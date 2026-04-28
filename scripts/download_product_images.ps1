param(
    [switch]$Force,
    [string]$OutDir = ".\Resources\Images"
)

function Slug($text) {
    $text = $text.ToLower()
    $text = $text.Replace("á","a").Replace("é","e").Replace("í","i").Replace("ó","o").Replace("ú","u").Replace("ñ","n")
    $text = $text.Replace(" ","-")
    return ($text -replace "[^a-z0-9\-]", "")
}

if (!(Test-Path $OutDir)) {
    New-Item -ItemType Directory -Path $OutDir -Force | Out-Null
}

$headers = @{
    "User-Agent" = "Mozilla/5.0"
}

$products = @(
    @{ Categoria="Bebidas"; Nombre="Cola"; Query="cola,soda,can" },
    @{ Categoria="Bebidas"; Nombre="Naranja"; Query="orange,soda,bottle" },
    @{ Categoria="Bebidas"; Nombre="Limón"; Query="lemon,soda,bottle" },
    @{ Categoria="Bebidas"; Nombre="Mineral"; Query="mineral,water,bottle" },

    @{ Categoria="Snacks"; Nombre="Sabritas"; Query="potato,chips,bag" },
    @{ Categoria="Snacks"; Nombre="Doritos"; Query="nachos,chips,bag" },
    @{ Categoria="Snacks"; Nombre="Cheetos"; Query="cheese,snack,chips" },
    @{ Categoria="Snacks"; Nombre="Takis"; Query="spicy,chips,snack" },

    @{ Categoria="Dulces"; Nombre="Paleta"; Query="lollipop,candy" },
    @{ Categoria="Dulces"; Nombre="Panditas"; Query="gummy,bears,candy" },
    @{ Categoria="Dulces"; Nombre="Caramelo"; Query="wrapped,candy" },

    @{ Categoria="Chocolates"; Nombre="Snickers"; Query="chocolate,bar" },
    @{ Categoria="Chocolates"; Nombre="KitKat"; Query="chocolate,wafer,bar" },
    @{ Categoria="Chocolates"; Nombre="Ferrero"; Query="chocolate,candy" },

    @{ Categoria="Galletas"; Nombre="Oreo"; Query="cookies,sandwich" },
    @{ Categoria="Galletas"; Nombre="Chokis"; Query="chocolate,chip,cookies" },

    @{ Categoria="Pan y Pastelitos"; Nombre="Concha"; Query="mexican,sweet,bread" },
    @{ Categoria="Pan y Pastelitos"; Nombre="Donita"; Query="donut,pastry" },

    @{ Categoria="Repostería"; Nombre="Brownie"; Query="brownie,dessert" },
    @{ Categoria="Repostería"; Nombre="Cupcake Vainilla"; Query="vanilla,cupcake" }
)

foreach ($p in $products) {
    $fileName = "$(Slug $p.Categoria)-$(Slug $p.Nombre).jpg"
    $path = Join-Path $OutDir $fileName

    if ((Test-Path $path) -and (-not $Force)) {
        Write-Host "Ya existe: $fileName"
        continue
    }

    $random = Get-Random -Minimum 1000 -Maximum 9999
    $url = "https://loremflickr.com/800/800/$($p.Query)?random=$random"

    Write-Host "Descargando: $fileName"

    try {
        Invoke-WebRequest -Uri $url -OutFile $path -Headers $headers -UseBasicParsing -ErrorAction Stop
        Start-Sleep -Seconds 1
    }
    catch {
        Write-Host ("Fallo imagen real, creando placeholder: " + $fileName) -ForegroundColor Yellow

        $texto = [uri]::EscapeDataString($p.Nombre)
        $fallback = "https://placehold.co/800x800/111111/FFFFFF.jpg?text=$texto"

        try {
            Invoke-WebRequest -Uri $fallback -OutFile $path -Headers $headers -UseBasicParsing -ErrorAction Stop
        }
        catch {
            Write-Host ("No se pudo crear: " + $fileName) -ForegroundColor Red
        }
    }
}

Write-Host "Listo. Haz Clean y Rebuild en Visual Studio." -ForegroundColor Green