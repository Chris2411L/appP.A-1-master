using System;
using System.Collections.Generic;
using System.Linq;

namespace appP.A.Models
{
    public static class AppData
    {
        public static List<Categoria> Categorias { get; set; } = new();
        public static Carrito CarritoActual { get; set; } = new Carrito();
        public static bool IsAdmin { get; set; } = false;

        private static int _nextProductId = 1;
        public static int GetNextProductId() => _nextProductId++;

        // 👇 AQUI VA TU FUNCION
        static string ImagenPorProducto(string categoria, string variante)
        {
            var key = $"{categoria}|{variante}".ToLower();

            var imagenes = new Dictionary<string, string>
    {
        // BEBIDAS
        ["bebidas|cola"] = "coca cola lata refresco",
        ["bebidas|naranja"] = "refresco naranja botella",
        ["bebidas|limón"] = "refresco limon botella",
        ["bebidas|manzana"] = "jugo manzana botella",
        ["bebidas|uva"] = "jugo uva botella",
        ["bebidas|tamarindo"] = "bebida tamarindo",
        ["bebidas|mango"] = "jugo mango botella",
        ["bebidas|durazno"] = "jugo durazno botella",
        ["bebidas|mineral"] = "agua mineral botella",
        ["bebidas|energética"] = "bebida energetica lata",
        ["bebidas|té verde"] = "te verde botella",
        ["bebidas|té negro"] = "te negro botella",
        ["bebidas|café frío"] = "cafe frio botella",
        ["bebidas|horchata"] = "agua horchata vaso",
        ["bebidas|jamaica"] = "agua jamaica vaso",

        // SNACKS
        ["snacks|sabritas"] = "bolsa papas fritas",
        ["snacks|doritos"] = "doritos bolsa",
        ["snacks|ruffles"] = "ruffles papas bolsa",
        ["snacks|cheetos"] = "cheetos bolsa",
        ["snacks|takis"] = "takis bolsa",
        ["snacks|rancheritos"] = "botana rancheritos bolsa",
        ["snacks|crujitos"] = "crujitos botana bolsa",
        ["snacks|totis"] = "totis botana",
        ["snacks|fritos"] = "fritos chips bolsa",
        ["snacks|churritos"] = "churritos snack",
        ["snacks|papas adobadas"] = "papas adobadas bolsa",
        ["snacks|jalapeño"] = "chips jalapeno",
        ["snacks|queso"] = "chips queso bolsa",
        ["snacks|limoncito"] = "chips limon bolsa",
        ["snacks|mix botanero"] = "mix botanero snacks",

        // DULCES
        ["dulces|paleta"] = "paleta dulce",
        ["dulces|pulparindo"] = "dulce tamarindo pulparindo",
        ["dulces|panditas"] = "gomitas panditas",
        ["dulces|mazapán"] = "mazapan dulce",
        ["dulces|skittles"] = "skittles candy",
        ["dulces|jolly"] = "jolly rancher candy",
        ["dulces|halls"] = "halls candy",
        ["dulces|rockaleta"] = "rockaleta paleta",
        ["dulces|pelón"] = "pelon pelo rico",
        ["dulces|lucas"] = "lucas candy",
        ["dulces|tamborcito"] = "tamborcito dulce",
        ["dulces|cachetada"] = "cachetada dulce",
        ["dulces|caramelo"] = "caramelos envueltos",
        ["dulces|malvavisco"] = "malvaviscos",
        ["dulces|dulce de leche"] = "dulce de leche candy",

        // CHOCOLATES
        ["chocolates|snickers"] = "snickers chocolate",
        ["chocolates|kitkat"] = "kitkat chocolate",
        ["chocolates|crunch"] = "crunch chocolate bar",
        ["chocolates|kisses"] = "hershey kisses",
        ["chocolates|carlos v"] = "carlos v chocolate",
        ["chocolates|abuelita"] = "chocolate abuelita",
        ["chocolates|milky way"] = "milky way chocolate",
        ["chocolates|ferrero"] = "ferrero rocher",
        ["chocolates|bubu lubu"] = "bubu lubu chocolate",
        ["chocolates|gansito"] = "gansito chocolate",
        ["chocolates|kinder"] = "kinder chocolate",
        ["chocolates|amargo 70%"] = "chocolate amargo 70",
        ["chocolates|crema cacahuate"] = "chocolate crema cacahuate",
        ["chocolates|avellana"] = "chocolate avellana",
        ["chocolates|menta"] = "chocolate menta",

        // ENCHILADOS
        ["enchilados|tamarindo"] = "dulce tamarindo chile",
        ["enchilados|mango"] = "mango enchilado",
        ["enchilados|sandía"] = "sandia enchilada",
        ["enchilados|chamoy"] = "chamoy dulce",
        ["enchilados|miguelito"] = "miguelito chile polvo",
        ["enchilados|limón"] = "dulce limon chile",
        ["enchilados|piña"] = "pina enchilada",
        ["enchilados|pepino"] = "pepino con chile",
        ["enchilados|guayaba"] = "guayaba enchilada",
        ["enchilados|ciruela"] = "ciruela enchilada",
        ["enchilados|manzana verde"] = "manzana verde enchilada",
        ["enchilados|durazno"] = "durazno enchilado",
        ["enchilados|pepino-chile"] = "pepino chile",
        ["enchilados|piña-chile"] = "pina chile",
        ["enchilados|chile-limón"] = "chile limon dulce",

        // GOMITAS
        ["gomitas|ositos"] = "gomitas ositos",
        ["gomitas|aros"] = "gomitas aros",
        ["gomitas|gusanitos"] = "gomitas gusanos",
        ["gomitas|frutas"] = "gomitas frutas",
        ["gomitas|ácidas"] = "gomitas acidas",
        ["gomitas|corazones"] = "gomitas corazones",
        ["gomitas|colas"] = "gomitas cola",
        ["gomitas|surtidas"] = "gomitas surtidas",
        ["gomitas|sandía"] = "gomitas sandia",
        ["gomitas|mango"] = "gomitas mango",
        ["gomitas|arándano"] = "gomitas arandano",
        ["gomitas|cereza"] = "gomitas cereza",
        ["gomitas|durazno"] = "gomitas durazno",
        ["gomitas|piña"] = "gomitas pina",
        ["gomitas|uva"] = "gomitas uva",

        // GALLETAS
        ["galletas|emperador limón"] = "galletas emperador limon",
        ["galletas|chokis"] = "galletas chokis",
        ["galletas|marías"] = "galletas marias",
        ["galletas|oreo"] = "galletas oreo",
        ["galletas|príncipe"] = "galletas principe",
        ["galletas|triki trakes"] = "triki trakes galletas",
        ["galletas|canelitas"] = "galletas canelitas",
        ["galletas|habaneras"] = "galletas habaneras",
        ["galletas|animalitos"] = "galletas animalitos",
        ["galletas|surtido rico"] = "galletas surtido rico",
        ["galletas|mantequilla"] = "galletas mantequilla",
        ["galletas|avena"] = "galletas avena",
        ["galletas|chocolate"] = "galletas chocolate",
        ["galletas|rellenas"] = "galletas rellenas",
        ["galletas|vainilla"] = "galletas vainilla",

        // CHICLES
        ["chicles|trident"] = "chicles trident",
        ["chicles|clorets"] = "chicles clorets",
        ["chicles|bubbaloo"] = "bubbaloo chicle",
        ["chicles|motita"] = "motita chicle",
        ["chicles|orbit"] = "orbit gum",
        ["chicles|doublemint"] = "doublemint gum",
        ["chicles|bigtime"] = "bigtime gum",
        ["chicles|drops"] = "chicle drops",
        ["chicles|menta"] = "chicle menta",
        ["chicles|hierbabuena"] = "chicle hierbabuena",
        ["chicles|fresa"] = "chicle fresa",
        ["chicles|uva"] = "chicle uva",
        ["chicles|sandía"] = "chicle sandia",
        ["chicles|mora azul"] = "chicle mora azul",
        ["chicles|canela"] = "chicle canela",

        // IMPORTADOS
        ["importados|pocky"] = "pocky snack",
        ["importados|hi-chew"] = "hi chew candy",
        ["importados|kitkat japón"] = "kitkat japan",
        ["importados|twix"] = "twix chocolate",
        ["importados|butterfinger"] = "butterfinger chocolate",
        ["importados|reese’s"] = "reeses chocolate",
        ["importados|mentos"] = "mentos candy",
        ["importados|haribo"] = "haribo gummies",
        ["importados|toffee"] = "toffee candy",
        ["importados|turco lokum"] = "turkish delight",
        ["importados|choco pie"] = "choco pie",
        ["importados|pepero"] = "pepero snack",
        ["importados|milkita"] = "milkita candy",
        ["importados|laffy taffy"] = "laffy taffy",

        // PAN
        ["pan y pastelitos|concha"] = "pan dulce concha",
        ["pan y pastelitos|mantecadas"] = "mantecadas pan",
        ["pan y pastelitos|gansito pan"] = "gansito pastelito",
        ["pan y pastelitos|roles canela"] = "roles de canela",
        ["pan y pastelitos|napo"] = "napolitano pastelito",
        ["pan y pastelitos|magdalena"] = "magdalena pan",
        ["pan y pastelitos|donita"] = "donas pan dulce",
        ["pan y pastelitos|panquesito marmoleado"] = "panque marmoleado",
        ["pan y pastelitos|cuernito dulce"] = "cuernito pan dulce",
        ["pan y pastelitos|oreja"] = "pan dulce oreja",
        ["pan y pastelitos|pan de elote"] = "pan de elote",
        ["pan y pastelitos|berlín"] = "berlin pastry",
        ["pan y pastelitos|panqué vainilla"] = "panque vainilla",
        ["pan y pastelitos|panqué choco"] = "panque chocolate",
        ["pan y pastelitos|panqué nuez"] = "panque nuez",

        // REPOSTERIA
        ["repostería|brownie"] = "brownie postre",
        ["repostería|pay de queso"] = "pay de queso",
        ["repostería|pay de limón"] = "pay de limon",
        ["repostería|cheesecake fresa"] = "cheesecake fresa",
        ["repostería|flan"] = "flan postre",
        ["repostería|gelatina mosaico"] = "gelatina mosaico",
        ["repostería|cupcake vainilla"] = "cupcake vainilla",
        ["repostería|cupcake choco"] = "cupcake chocolate",
        ["repostería|cupcake redvelvet"] = "red velvet cupcake",
        ["repostería|alfajor"] = "alfajor",
        ["repostería|macaron"] = "macaron",
        ["repostería|rol de guayaba"] = "rol guayaba",
        ["repostería|rol de zarzamora"] = "rol zarzamora",
        ["repostería|tres leches"] = "pastel tres leches",
        ["repostería|tiramisú"] = "tiramisu",

        // COMBOS
        ["combos y ofertas|combo escolar"] = "lunch snack combo",
        ["combos y ofertas|combo fiesta"] = "party snack combo",
        ["combos y ofertas|combo gamer"] = "gaming snacks",
        ["combos y ofertas|combo cine"] = "movie snacks popcorn",
        ["combos y ofertas|combo oficina"] = "office snacks",
        ["combos y ofertas|combo viaje"] = "travel snacks",
        ["combos y ofertas|combo dulcero"] = "candy box",
        ["combos y ofertas|combo picante"] = "spicy snack combo",
        ["combos y ofertas|combo kids"] = "kids snack box",
        ["combos y ofertas|combo premium"] = "premium snack box",
        ["combos y ofertas|combo mix"] = "mixed snacks",
        ["combos y ofertas|combo para 2"] = "snacks for two",
        ["combos y ofertas|combo familiar"] = "family snack box",
        ["combos y ofertas|combo ahorro"] = "discount snack pack",
        ["combos y ofertas|combo sorpresa"] = "surprise snack box"
    };

              string query = imagenes.ContainsKey(key)
        ? imagenes[key]
        : $"{variante} {categoria} producto";

    return $"https://tse.mm.bing.net/th?q={Uri.EscapeDataString(query)}&w=800&h=800&c=7&rs=1&p=0&o=5&pid=1.7";
        }
    }
}
