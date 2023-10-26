using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;

public class Program
{
    public static void Main()
    {
        using (var ctx = new ExampleContext())
        {
            ctx.Database.Migrate();
        }
        SetupData();

        WeirdBehaviour();
        PrintPerson();

        Cleanup();
    }
    private static void WeirdBehaviour()
    {
        using var ctx = new ExampleContext();
        var person = ctx.People.First();
        person.IdCard = new IdCard() { FullName = "Another John Doe", Country = "Germany" };

        ctx.SaveChanges();
    }

    private static void PrintPerson()
    {
        using var ctx = new ExampleContext();
        var person = ctx.People.First();
        Console.WriteLine(person);
    }

    private static void Cleanup()
    {
        using var ctx = new ExampleContext();
        ctx.People.ExecuteDelete();
    }
    private static void SetupData()
    {
        using var ctx = new ExampleContext();
        var person = new Person();
        person.IdCard = new IdCard() { FullName = "Original John Doe", Country = "Germany" };
        ctx.People.Add(person);
        ctx.SaveChanges();
    }
}

public class Person : INotifyPropertyChanged, INotifyPropertyChanging
{
    private long _id;

    public long Id
    {
        get => _id;
        set
        {
            if (_id != value)
            {
                this.OnPropertyChanging();
                this._id = value;
                this.OnPropertyChanged();
            }
        }
    }

    private IdCard _idCard;

    public IdCard IdCard
    {
        get => _idCard;
        set
        {

            if (_idCard != value)
            {
                this.OnPropertyChanging();
                this._idCard = value;
                this.OnPropertyChanged();
            }
        }
    }

    protected readonly Action<object, string> lazyLoader;

    public Person()
    {

    }

    public Person(Action<object, string> lazyLoader)
    {
        this.lazyLoader = lazyLoader;
    }

    public event PropertyChangingEventHandler PropertyChanging;
    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanging([CallerMemberName] string propertyName = null)
    {
        if (PropertyChanging != null)
        {
            PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }
    }
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

public class IdCard : INotifyPropertyChanged, INotifyPropertyChanging
{
    private string _fullName;

    public string FullName
    {
        get => _fullName;
        set
        {
            if (_fullName != value)
            {
                this.OnPropertyChanging();
                this._fullName = value;
                this.OnPropertyChanged();
            }
        }
    }

    private string _country;

    public string Country
    {
        get => _country;
        set
        {
            if (_country != value)
            {
                this.OnPropertyChanging();
                this._country = value;
                this.OnPropertyChanged();
            }
        }
    }

    public event PropertyChangingEventHandler PropertyChanging;
    public event PropertyChangedEventHandler PropertyChanged;

    public virtual void OnPropertyChanging([CallerMemberName] string propertyName = null)
    {
        if (PropertyChanging != null)
        {
            PropertyChanging(this, new PropertyChangingEventArgs(propertyName));
        }
    }
    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

public class ExampleContext : DbContext
{
    public DbSet<Person> People { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer("Server=.;Database=WeirdBehaviour;User Id=dev;Password=admin;TrustServerCertificate=True;Trusted_Connection=True");
        optionsBuilder.EnableDetailedErrors();
        optionsBuilder.EnableSensitiveDataLogging();
        optionsBuilder.EnableThreadSafetyChecks();
        optionsBuilder.LogTo(x =>
        {
            Console.WriteLine(x);
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);

        modelBuilder.Entity<Person>()
            .OwnsOne(x => x.IdCard);
    }
}


public static class PocoLoadingExtensions
{
    public static ICollection<TRelated> Load<TRelated>(
        this Action<object, string> loader,
        object entity,
        ref ICollection<TRelated> navigationField,
        [CallerMemberName] string navigationName = null)
        where TRelated : class
    {
        loader?.Invoke(entity, navigationName);
        navigationField ??= new ObservableCollection<TRelated>();

        return navigationField;
    }
}
