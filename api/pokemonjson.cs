
public class PokemonStatsJson {
    public required string Name {get;set;}
    public  int Value {get;set;}
}

public class PokemonEvolutionJson {
    public  string? From {get;set;}
    public  List<string>? To {get;set;}
}

public class PokemonJson {
    public int Number {get ;set;}
    public required string Name {get;set;}
    public required string Generation {get;set;}
    public  int Height {get;set;}
    public  int Weight {get;set;}
    public required List<string> Types {get;set;}
    public required List<PokemonStatsJson> Stats {get;set;}
    public required List<string> Moves {get;set;}
    public required List<string> Abilities {get;set;}
    public required PokemonEvolutionJson? Evolution {get;set;}
    public required string Image {get;set;}
}

