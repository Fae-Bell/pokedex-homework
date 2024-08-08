using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Text.Json;

public class PokemonDb : DbContext {
    public DbSet<Pokemon> PokemonData => Set<Pokemon>();
    public DbSet<PokemonAbility> PokemonAbilities => Set<PokemonAbility>();
    public DbSet<PokemonType> PokemonTypes => Set<PokemonType>();


    public PokemonDb(DbContextOptions<PokemonDb> options) :base(options) {    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var data = LoadJsonData();
        data.Wait();
        if (!data.IsCompletedSuccessfully || data.Result == null) {
            throw new Exception("Invalid data from file");
        }
        var abilities = data.Result.SelectMany(r => r.Abilities).Distinct();
        var abilitiesJoined = abilities.SelectMany(t => t.Pokemon.Select(p => new { PokemonNumber = p.Number, AbilitiesID = t.ID })).Distinct();
        var types = data.Result.SelectMany(r => r.Types).Distinct();
        var typeJoined = types.SelectMany(t => t.Pokemon.Select(p => new { PokemonNumber = p.Number, TypesID = t.ID })).Distinct();
        var moves = data.Result.SelectMany(r => r.Moves).Distinct();
        var moveJoined = moves.SelectMany(t => t.Pokemon.Select(p => new { PokemonNumber = p.Number, MovesID = t.ID })).Distinct();
        var stats = data.Result.SelectMany(r => r.Stats).Distinct();
        
        modelBuilder.Entity<PokemonAbility>(p => {
            p.HasKey(e => e.ID);
            p.HasData(abilities.Select(a => new { a.ID, a.Name }));
        });
        modelBuilder.Entity<PokemonMove>(p => {
            p.HasKey(e => e.ID);
            p.HasData(moves.Select(m => new { m.ID, m.Name }));
        });
        modelBuilder.Entity<PokemonType>(p => {
            p.HasKey(e => e.ID);
            p.HasData(types.Select(t => new { t.ID, t.Name }));
        });
        modelBuilder.Entity<PokemonStats>(p => {
            p.HasKey(e => new { e.PokemonNumber, e.Name});
            p.HasData(stats.Select(t => new { t.Name, t.PokemonNumber, t.Value }));
        });

        modelBuilder.Entity<Pokemon>(p => {
            p.HasKey(e => e.Number);
            p.HasIndex(e => e.Name);
            p.HasMany(e => e.Types)
                .WithMany(e => e.Pokemon)
                .UsingEntity("PokemonTypes", j => j.HasData(typeJoined));
            p.HasMany(e => e.Abilities)
                .WithMany(e => e.Pokemon)
                .UsingEntity("PokemonAbilities", j => j.HasData(abilitiesJoined));
            p.HasMany(e => e.Moves)
                .WithMany(e => e.Pokemon)
                .UsingEntity("PokemonMoves", j => j.HasData(moveJoined));
            p.HasMany(e => e.Stats)
                .WithOne(e => e.Pokemon)
                .HasForeignKey(e => e.PokemonNumber);
            p.HasOne(e => e.PreviousEvolution)
                .WithMany(e => e.FutureEvolutions)
                .HasForeignKey(e => e.PreviousEvolutionNumber)
                .IsRequired(false);
            p.HasData(data.Result.Select(r => new {
                r.Number,
                r.Name,
                r.Generation,
                r.Height,
                r.Weight,
                r.Image,
                PreviousEvolutionNumber = r.PreviousEvolution?.Number,
            }));
        });
    }

    public static async Task<List<Pokemon>?> LoadJsonData() {
        List<PokemonJson>? storedData = null;
        using (FileStream data = File.OpenRead("data/pokemon.json")) {
            storedData = await JsonSerializer.DeserializeAsync<List<PokemonJson>>(data, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }
        if (storedData == null) {
            return null;
        }

        Dictionary<string, PokemonType> types = [];
        int typesID = 1;
        Dictionary<string, PokemonAbility> abilities = [];
        int abilitiesID = 1;
        Dictionary<string, PokemonMove> moves = [];
        int movesID = 1;
        Dictionary<string, Pokemon> allPokemon = [];

        storedData.ForEach(d => {
            var dataTypes = d.Types.Select(t => {
                t = t.ToLower();
                if (!types.TryGetValue(t, out PokemonType? type)) {
                    type = new PokemonType { ID = typesID++, Name = t, Pokemon = [] };
                    types[t] = type;
                }
                return type;
            }).ToList();
            var dataAbilities = d.Abilities.Select(t => {
                t = t.ToLower();
                if (!abilities.TryGetValue(t, out PokemonAbility? ability)) {
                    ability = new PokemonAbility { ID = abilitiesID++, Name = t, Pokemon = [] };
                    abilities[t] = ability;
                }
                return ability;
            }).ToList();
            var dataMoves = d.Moves.Select(t => {
                t = t.ToLower();
                if (!moves.TryGetValue(t, out PokemonMove? move)) {
                    move = new PokemonMove { ID = movesID++, Name = t, Pokemon = [] };
                    moves[t] = move;
                }
                return move;
            }).ToList();

            var key = NameToKey(d.Name);

            var pokemon = new Pokemon {
                Number = d.Number,
                Name = d.Name,
                Height = d.Height,
                Weight = d.Weight,
                Generation = d.Generation,
                Types = dataTypes,
                Abilities = dataAbilities,
                Moves = dataMoves,
                Stats = [],
                Image = d.Image,
                PreviousEvolution = null,
                FutureEvolutions = null,
            };
            pokemon.Stats = d.Stats.Select(s => new PokemonStats{ Name = s.Name, Value = s.Value, PokemonNumber = pokemon.Number }).ToList();
            dataAbilities.ForEach(a => {
                a.Pokemon.Add(pokemon);
                abilities[a.Name] = a;
            });
            dataMoves.ForEach(a => {
                a.Pokemon.Add(pokemon);
                moves[a.Name] = a;
            });
            dataTypes.ForEach(a => {
                a.Pokemon.Add(pokemon);
                types[a.Name] = a;
            });
            allPokemon[key] = pokemon;
        });
        var normalizedData = storedData.Select(d => {
            var key = NameToKey(d.Name);
            var p = allPokemon[key];
            if (d.Evolution != null) {
                List<Pokemon>? to = null;
                Pokemon? from = null;
                if (d.Evolution.To != null) {
                    to = d.Evolution.To.Select(f => allPokemon[f.ToLower()]).ToList();
                }
                if (d.Evolution.From != null) {
                    from = allPokemon[d.Evolution.From.ToLower()];
                }
                if (from != null) {
                    p.PreviousEvolution = from;
                }
                if (to != null) {
                    p.FutureEvolutions = to;
                }
            }
            return p;
        }).ToList();


        return normalizedData;
    }

    private static string NameToKey(string s) {
        var key = s.ToLower();
        if (key == "nidoran\u2642") {
            key = "nidoran-m";
        }
        if (key == "nidoran\u2640") {
            key = "nidoran-f";
        }
        if (key == "flab\u00E9b\u00E9") {
            key = "flabebe";
        }
        if (key == "farfetch\u2019d") {
            key = "farfetchd";
        }
        if (key == "sirfetch\u2019d") {
            key = "sirfetchd";
        }
        if (key == "mime jr.") {
            key = "mime-jr";
        }
        if (key == "mr. mime") {
            key = "mr-mime";
        }
        if (key == "mr. rime") {
            key = "mr-rime";
        }
        if (key == "type: null") {
            key = "type-null";
        }

        return key;
    }
}