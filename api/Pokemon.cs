
using System.Text.Json.Serialization;

public class PokemonType {
    public int ID {get;set;}
    public required string Name {get;set;}
    [JsonIgnore]
    public virtual ICollection<Pokemon> Pokemon {get;set;} = null!;
}

public class PokemonMove {
    public int ID {get;set;}
    public required string Name {get;set;}
    [JsonIgnore]
    public virtual ICollection<Pokemon> Pokemon {get;set;} = null!;
}

public class PokemonAbility {
    public int ID {get;set;}
    public required string Name {get;set;}
    [JsonIgnore]
    public virtual ICollection<Pokemon> Pokemon {get;set;} = null!;
}

public class PokemonStats {
    public required string Name {get;set;}
    public int Value {get;set;}
    [JsonIgnore]
    public int PokemonNumber {get;set;}
    [JsonIgnore]
    public virtual Pokemon? Pokemon {get;set;}
}

public class Pokemon {
    public int Number {get ;set;}
    public required string Name {get;set;}
    public required string Generation {get;set;}
    public  int Height {get;set;}
    public  int Weight {get;set;}
    public required ICollection<PokemonType> Types {get;set;}
    public required ICollection<PokemonMove> Moves {get;set;}
    public required ICollection<PokemonAbility> Abilities {get;set;}
    public required List<PokemonStats> Stats {get;set;}
    [JsonIgnore]
    public int? PreviousEvolutionNumber {get;set;}
    public required Pokemon? PreviousEvolution {get;set;}
    public required List<Pokemon>? FutureEvolutions {get;set;}
    public required string Image {get;set;}
}
