<#
Script: download_product_images.ps1
Purpose: descargar imágenes alusivas automáticamente desde source.unsplash.com
Usage (desde la raíz del repo):
  powershell -NoProfile -ExecutionPolicy Bypass -File .\scripts\download_product_images.ps1 [-Force]

Opciones:
  -Force        : sobrescribe imágenes existentes
  -DelaySeconds : tiempo de espera entre descargas (por defecto 1)
  -Width/Height : tamaño solicitado a Unsplash (por defecto 800x800)
  -OutDir       : carpeta destino (por defecto .\Resources\Images)
#>

param(
    [switch]$Force,
    [int]$DelaySeconds = 1,
    [int]$Width = 800,
    [int]$Height = 800,
    [string]$OutDir = ".\Resources\Images"
)

$ErrorActionPreference = 'Continue'

function Slug([string]$s) {
    if (-not $s) { return "" }
    $map = @{ 'á'='a'; 'é'='e'; 'í'='i'; 'ó'='o'; 'ú'='u'; 'ñ'='n'; '’'=''; "'"=''; '"'=''; ' '='-' }
    $s = $s.ToLower()
    foreach ($k in $map.Keys) { $s = $s -replace [regex]::Escape($k), $map[$k] }
    return ($s -replace '[^a-z0-9\-]', '')
}

# Base Unsplash (sin API)
$baseUrl = "https://source.unsplash.com/${Width}x${Height}/?"

# Map de categorías y variantes (mantener sincronizado con Models/AppData.cs si cambias)
$map = @{
    'Bebidas' = @('Cola','Naranja','Limón','Manzana','Uva','Tamarindo','Mango','Durazno','Mineral','Energética','Té Verde','Té Negro','Café Frío','Horchata','Jamaica')
    'Snacks' = @('Sabritas','Doritos','Ruffles','Cheetos','Takis','Rancheritos','Crujitos','Totis','Fritos','Churritos','Papas Adobadas','Jalapeño','Queso','Limoncito','Mix Botanero')
    'Dulces' = @('Paleta','Pulparindo','Panditas','Mazapán','Skittles','Jolly','Halls','Rockaleta','Pelón','Lucas','Tamborcito','Cachetada','Caramelo','Malvavisco','Dulce de Leche')
    'Chocolates' = @('Snickers','KitKat','Crunch','Kisses','Carlos V','Abuelita','Milky Way','Ferrero','Bubu Lubu','Gansito','Kinder','Amargo 70%','Crema Cacahuate','Avellana','Menta')
    'Enchilados' = @('Tamarindo','Mango','Sandía','Chamoy','Miguelito','Limón','Piña','Pepino','Guayaba','Ciruela','Manzana Verde','Durazno','Pepino-Chile','Piña-Chile','Chile-Limón')
    'Gomitas' = @('Ositos','Aros','Gusanitos','Frutas','Ácidas','Corazones','Colas','Surtidas','Sandía','Mango','Arándano','Cereza','Durazno','Piña','Uva')
    'Galletas' = @('Emperador Limón','Chokis','Marías','Oreo','Príncipe','Triki Trakes','Canelitas','Habaneras','Animalitos','Surtido Rico','Mantequilla','Avena','Chocolate','Rellenas','Vainilla')
    'Chicles' = @('Trident','Clorets','Bubbaloo','Motita','Orbit','Doublemint','BigTime','Drops','Menta','Hierbabuena','Fresa','Uva','Sandía','Mora Azul','Canela')
    'Importados' = @('Pocky','Hi-Chew','KitKat Japón','Twix','Butterfinger','Reese’s','Mentos','Haribo','Toffee','Turco Lokum','Choco Pie','Pepero','Milkita','Laffy Taffy')
    'Pan y Pastelitos' = @('Concha','Mantecadas','Gansito Pan','Roles Canela','Napo','Magdalena','Donita','Panquesito Marmoleado','Cuernito Dulce','Oreja','Pan de Elote','Berlín','Panqué Vainilla','Panqué Choco','Panqué Nuez')
    'Repostería' = @('Brownie','Pay de Queso','Pay de Limón','Cheesecake Fresa','Flan','Gelatina Mosaico','Cupcake Vainilla','Cupcake Choco','Cupcake RedVelvet','Alfajor','Macaron','Rol de Guayaba','Rol de Zarzamora','Tres Leches','Tiramisú')
    'Combos y Ofertas' = @('Combo Escolar','Combo Fiesta','Combo Gamer','Combo Cine','Combo Oficina','Combo Viaje','Combo Dulcero','Combo Picante','Combo Kids','Combo Premium','Combo Mix','Combo Para 2','Combo Familiar','Combo Ahorro','Combo Sorpresa')
}

# Crear carpeta de salida si no existe
$absOut = Resolve-Path -Path $OutDir -ErrorAction SilentlyContinue
if (-not $absOut) { New-Item -ItemType Directory -Path $OutDir -Force | Out-Null; $absOut = Resolve-Path -Path $OutDir }
$absOut = $absOut.Path

$created = 0; $skipped = 0; $failed = 0

foreach ($cat in $map.Keys) {
    foreach ($v in $map[$cat]) {
        $filename = "{0}-{1}.jpg" -f (Slug($cat)), (Slug($v))
        $path = Join-Path -Path $absOut -ChildPath $filename

        if ((Test-Path $path) -and (-not $Force)) {
            Write-Host "Existe (omitido): $filename"
            $skipped++
            continue
        }

        $query = [System.Web.HttpUtility]::UrlEncode("$v, $cat, product")
        $url = "$baseUrl$query"

        Write-Host "Descargando: $filename desde $url"
        try {
            Invoke-WebRequest -Uri $url -OutFile $path -UseBasicParsing -ErrorAction Stop
            Start-Sleep -Seconds $DelaySeconds
            $created++
        } catch {
            Write-Host "Error descargando $filename: $_" -ForegroundColor Red
            $failed++
            # crear placeholder mínimo si falla
            try { Set-Content -Path $path -Value "placeholder" -Encoding UTF8 } catch {}
        }
    }
}

Write-Host "Terminado. Creado: $created, Omitidos: $skipped, Fallos: $failed" -ForegroundColor Green
Write-Host "Rebuild la solución en Visual Studio para empaquetar las imágenes." 
$abs = Resolve-Path -Path $ImagesDir -ErrorAction SilentlyContinue
if (-not $abs) { Write-Host "No existe $ImagesDir"; exit }
$abs = $abs.Path

Write-Host "Borrando archivos en $abs ..."
Get-ChildItem -Path $abs -File | Remove-Item -Force -Verbose
Write-Host "Hecho."

param([string]$ImagesDir = ".\Resources\Images")

$abs = Resolve-Path -Path $ImagesDir -ErrorAction SilentlyContinue
if (-not $abs) { Write-Host "No existe $ImagesDir"; exit }
$abs = $abs.Path

Write-Host "Borrando archivos en $abs ..."
Get-ChildItem -Path $abs -File | Remove-Item -Force -Verbose
Write-Host "Hecho."

param([string]$ImagesDir = ".\Resources\Images")

$abs = Resolve-Path -Path $ImagesDir -ErrorAction SilentlyContinue
if (-not $abs) { Write-Host "No existe $ImagesDir"; exit }
$abs = $abs.Path

Write-Host "Borrando archivos en $abs ..."
Get-ChildItem -Path $abs -File | Remove-Item -Force -Verbose
Write-Host "Hecho."

param([string]$ImagesDir = ".\Resources\Images")

$abs = Resolve-Path -Path $ImagesDir -ErrorAction SilentlyContinue
if (-not $abs) { Write-Host "No existe $ImagesDir"; exit }
$abs = $abs.Path

Write-Host "Borrando archivos en $abs ..."
Get-ChildItem -Path $abs -File | Remove-Item -Force -Verbose
Write-Host "Hecho."

param([string]$ImagesDir = ".\Resources\Images")

$abs = Resolve-Path -Path $ImagesDir -ErrorAction SilentlyContinue
if (-not $abs) { Write-Host "No existe $ImagesDir"; exit }
$abs = $abs.Path

Write-Host "Borrando archivos en $abs ..."
Get-ChildItem -Path $abs -File | Remove-Item -Force -Verbose
Write-Host "Hecho."

param([string]$ImagesDir = ".\Resources\Images")

$abs = Resolve-Path -Path $ImagesDir -ErrorAction SilentlyContinue
if (-not $abs) { Write-Host "No existe $ImagesDir"; exit }
$abs = $abs.Path

Write-Host "Borrando archivos en $abs ..."
Get-ChildItem -Path $abs -File | Remove-Item -Force -Verbose
Write-Host "Hecho."

param([string]$ImagesDir = ".\Resources\Images")

$abs = Resolve-Path -Path $ImagesDir -ErrorAction SilentlyContinue
if (-not $abs) { Write-Host "No existe $ImagesDir"; exit }
$abs = $abs.Path

Write-Host "Borrando archivos en $abs ..."
Get-ChildItem -Path $abs -File | Remove-Item -Force -Verbose
Write-Host "Hecho."

param([string]$ImagesDir = ".\Resources\Images")

$abs = Resolve-Path -Path $ImagesDir -ErrorAction SilentlyContinue
if (-not $abs) { Write-Host "No existe $ImagesDir"; exit }
$abs = $abs.Path

Write-Host "Borrando archivos en $abs ..."
Get-ChildItem -Path $abs -File | Remove-Item -Force -Verbose
Write-Host "Hecho."

param([string]$ImagesDir = ".\Resources\Images")

$abs = Resolve-Path -Path $ImagesDir -ErrorAction SilentlyContinue
if (-not $abs) { Write-Host "No existe $ImagesDir"; exit }
$abs = $abs.Path

Write-Host "Borrando archivos en $abs ..."
Get-ChildItem -Path $abs -File | Remove-Item -Force -Verbose
Write-Host "Hecho."

    param([string]$ImagesDir = ".\Resources\Images")

$abs = Resolve-Path -Path $ImagesDir -ErrorAction SilentlyContinue
if (-not $abs) { Write-Host "No existe $ImagesDir"; exit }
$abs = $abs.Path

Write-Host "Borrando archivos en $abs ..."
Get-ChildItem -Path $abs -File | Remove-Item -Force -Verbose
Write-Host "Hecho."

param([string]$ImagesDir = ".\Resources\Images")

$abs = Resolve-Path -Path $ImagesDir -ErrorAction SilentlyContinue
if (-not $abs) { Write-Host "No existe $ImagesDir"; exit }
$abs = $abs.Path

Write-Host "Borrando archivos en $abs ..."
Get-ChildItem -Path $abs -File | Remove-Item -Force -Verbose
Write-Host "Hecho."

param([string]$ImagesDir = ".\Resources\Images")

$abs = Resolve-Path -Path $ImagesDir -ErrorAction SilentlyContinue
if (-not $abs) { Write-Host "No existe $ImagesDir"; exit }
$abs = $abs.Path

Write-Host "Borrando archivos en $abs ..."
Get-ChildItem -Path $abs -File | Remove-Item -Force -Verbose
Write-Host "Hecho."

param([string]$ImagesDir = ".\Resources\Images")

$abs = Resolve-Path -Path $ImagesDir -ErrorAction SilentlyContinue
if (-not $abs) { Write-Host "No existe $ImagesDir"; exit }
$abs = $abs.Path

Write-Host "Borrando archivos en $abs ..."
Get-ChildItem -Path $abs -File | Remove-Item -Force -Verbose
Write-Host "Hecho."

param([string]$ImagesDir = ".\Resources\Images")

$abs = Resolve-Path -Path $ImagesDir -ErrorAction SilentlyContinue
if (-not $abs) { Write-Host "No existe $ImagesDir"; exit }
$abs = $abs.Path

Write-Host "Borrando archivos en $abs ..."
Get-ChildItem -Path $abs -File | Remove-Item -Force -Verbose
Write-Host "Hecho."

param([string]$ImagesDir = ".\Resources\Images")

$abs = Resolve-Path -Path $ImagesDir -ErrorAction SilentlyContinue
if (-not $abs) { Write-Host "No existe $ImagesDir"; exit }
$abs = $abs.Path

Write-Host "Borrando archivos en $abs ..."
Get-ChildItem -Path $abs -File | Remove-Item -Force -Verbose
Write-Host "Hecho."

    # Directorio donde se guardarán las imágenes
$imagesDir = Join-Path -Path $PSScriptRoot -ChildPath "..\Resources\Images"
$imagesDir = Resolve-Path -Path $imagesDir -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Path -ErrorAction SilentlyContinue
if (-not $imagesDir) {
    $imagesDir = Join-Path -Path $PSScriptRoot -ChildPath "..\Resources\Images"
    New-Item -ItemType Directory -Path $imagesDir -Force | Out-Null
}

Write-Host "Images directory: $imagesDir"

function Slug([string]$s) {
    if (-not $s) { return "" }
    $map = @{ 'á'='a'; 'é'='e'; 'í'='i'; 'ó'='o'; 'ú'='u'; 'ñ'='n'; '’'=''; "'"=''; '"'=''; ' '='-' }
    $s = $s.ToLower()
    foreach ($k in $map.Keys) { $s = $s -replace [regex]::Escape($k), $map[$k] }
    return ($s -replace '[^a-z0-9\-]', '')
}

# Base Unsplash (sin API)
$baseUrl = "https://source.unsplash.com/${Width}x${Height}/?"

# Map de categorías y variantes (mantenlo sincronizado con Models/AppData.cs si cambias)
$map = @{
    'Bebidas' = @('Cola','Naranja','Limón','Manzana','Uva','Tamarindo','Mango','Durazno','Mineral','Energética','Té Verde','Té Negro','Café Frío','Horchata','Jamaica')
    'Snacks' = @('Sabritas','Doritos','Ruffles','Cheetos','Takis','Rancheritos','Crujitos','Totis','Fritos','Churritos','Papas Adobadas','Jalapeño','Queso','Limoncito','Mix Botanero')
    'Dulces' = @('Paleta','Pulparindo','Panditas','Mazapán','Skittles','Jolly','Halls','Rockaleta','Pelón','Lucas','Tamborcito','Cachetada','Caramelo','Malvavisco','Dulce de Leche')
    'Chocolates' = @('Snickers','KitKat','Crunch','Kisses','Carlos V','Abuelita','Milky Way','Ferrero','Bubu Lubu','Gansito','Kinder','Amargo 70%','Crema Cacahuate','Avellana','Menta')
    'Enchilados' = @('Tamarindo','Mango','Sandía','Chamoy','Miguelito','Limón','Piña','Pepino','Guayaba','Ciruela','Manzana Verde','Durazno','Pepino-Chile','Piña-Chile','Chile-Limón')
    'Gomitas' = @('Ositos','Aros','Gusanitos','Frutas','Ácidas','Corazones','Colas','Surtidas','Sandía','Mango','Arándano','Cereza','Durazno','Piña','Uva')
    'Galletas' = @('Emperador Limón','Chokis','Marías','Oreo','Príncipe','Triki Trakes','Canelitas','Habaneras','Animalitos','Surtido Rico','Mantequilla','Avena','Chocolate','Rellenas','Vainilla')
    'Chicles' = @('Trident','Clorets','Bubbaloo','Motita','Orbit','Doublemint','BigTime','Drops','Menta','Hierbabuena','Fresa','Uva','Sandía','Mora Azul','Canela')
    'Importados' = @('Pocky','Hi-Chew','KitKat Japón','Twix','Butterfinger','Reese’s','Mentos','Haribo','Toffee','Turco Lokum','Choco Pie','Pepero','Milkita','Laffy Taffy')
    'Pan y Pastelitos' = @('Concha','Mantecadas','Gansito Pan','Roles Canela','Napo','Magdalena','Donita','Panquesito Marmoleado','Cuernito Dulce','Oreja','Pan de Elote','Berlín','Panqué Vainilla','Panqué Choco','Panqué Nuez')
    'Repostería' = @('Brownie','Pay de Queso','Pay de Limón','Cheesecake Fresa','Flan','Gelatina Mosaico','Cupcake Vainilla','Cupcake Choco','Cupcake RedVelvet','Alfajor','Macaron','Rol de Guayaba','Rol de Zarzamora','Tres Leches','Tiramisú')
    'Combos y Ofertas' = @('Combo Escolar','Combo Fiesta','Combo Gamer','Combo Cine','Combo Oficina','Combo Viaje','Combo Dulcero','Combo Picante','Combo Kids','Combo Premium','Combo Mix','Combo Para 2','Combo Familiar','Combo Ahorro','Combo Sorpresa')
}

# Crear carpeta de salida si no existe
$absOut = Resolve-Path -Path $OutDir -ErrorAction SilentlyContinue
if (-not $absOut) { New-Item -ItemType Directory -Path $OutDir -Force | Out-Null; $absOut = Resolve-Path -Path $OutDir }
$absOut = $absOut.Path

$created = 0; $skipped = 0; $failed = 0

foreach ($cat in $map.Keys) {
    foreach ($v in $map[$cat]) {
        $filename = "{0}-{1}.jpg" -f (Slug($cat)), (Slug($v))
        $path = Join-Path -Path $absOut -ChildPath $filename

        if ((Test-Path $path) -and (-not $Force)) {
            Write-Host "Existe (omitido): $filename"
            $skipped++
            continue
        }

        $query = [System.Web.HttpUtility]::UrlEncode("$v, $cat, product")
        $url = "$baseUrl$query"

        Write-Host "Descargando: $filename desde $url"
        try {
            Invoke-WebRequest -Uri $url -OutFile $path -UseBasicParsing -ErrorAction Stop
            Start-Sleep -Seconds $DelaySeconds
            $created++
        } catch {
            Write-Host "Error descargando $filename: $_" -ForegroundColor Red
            $failed++
            # crear placeholder mínimo (si falla)
            try { Set-Content -Path $path -Value "placeholder" -Encoding UTF8 } catch {}
        }
    }
}

Write-Host "Terminado. Creado: $created, Omitidos: $skipped, Fallos: $failed"
Write-Host "Rebuild la solución en Visual Studio para empaquetar las imágenes."
