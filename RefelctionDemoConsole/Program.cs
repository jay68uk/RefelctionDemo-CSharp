// See https://aka.ms/new-console-template for more information

using System.Reflection;
using System.Text.Json;

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Find all classes that inherit IApiModel");
Console.WriteLine("Should be on class ApiModelA and ApiModelB");

var assembly = Assembly.GetExecutingAssembly();
var types = assembly.GetTypes();

var apiModelInterfaceTypes = types.Where(t => t.GetInterfaces().Contains(typeof(IApiModel)));

Console.ForegroundColor = ConsoleColor.Yellow;
foreach (var type in apiModelInterfaceTypes) Console.WriteLine(type.Name);

Console.WriteLine();
Console.WriteLine();

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Find for that matches custom attribute:");
Console.WriteLine("ComponentNameAttribute = text_description_component");
Console.WriteLine("Should be on class ApiModelB");

//Use this to search all assemblies
//Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

const string desiredComponentName = "text_description_component";

var apiModelTypes = types.Where(t =>
    t.GetInterfaces().Contains(typeof(IApiModel)) && t.GetCustomAttributes<ComponentNameAttribute>()
        .Any(attr => attr.GetName() == desiredComponentName));

Console.ForegroundColor = ConsoleColor.Yellow;
foreach (var type in apiModelTypes) Console.WriteLine(type.Name);

Console.WriteLine();
Console.WriteLine();

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Create disctioanry of values for Component text_description_component");
Console.ForegroundColor = ConsoleColor.Cyan;
var propertyValues = new Dictionary<string, object>
{
    { "@Id", Guid.NewGuid() },
    { "@name", "Text description component" },
    { "description", "Populate the description using reflection." }
};


var apiModelType = types.FirstOrDefault(t => t.GetInterfaces().Contains(typeof(IApiModel)) && t
    .GetCustomAttributes<ComponentNameAttribute>()
    .Any(attr => attr.GetName() == desiredComponentName));

Console.WriteLine();

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Find the class and create an instance of it.");
var apiModelObject = Activator.CreateInstance(apiModelType!);

var properties = apiModelType!.GetProperties();

Console.WriteLine("Populate the properties.");
foreach (var property in properties)
{
    var dictionaryKeyAttribute = property.GetCustomAttribute<DictionaryKeyAttribute>();
    if (dictionaryKeyAttribute != null && propertyValues.TryGetValue(dictionaryKeyAttribute.Key, out var value))
    {
        if (value is Guid)
            value = Guid.Parse((string)value);

        property.SetValue(apiModelObject, value);
    }
}

var apiModel = Convert.ChangeType(apiModelObject, apiModelType) as IApiModel;
Console.ForegroundColor = ConsoleColor.Yellow;
Console.WriteLine("Id: " + apiModel!.Id);
Console.WriteLine("Name: " + apiModel.Name);

Console.WriteLine();
Console.WriteLine();

Console.ForegroundColor = ConsoleColor.Green;
Console.WriteLine("Build the output model and serialise to Json:");
var result = new ApiOutputModel { Name = "API Model", ExtraData = apiModel };

Console.ForegroundColor = ConsoleColor.Red;
Console.WriteLine(JsonSerializer.Serialize(result));
Console.ResetColor();

/// <summary>
///     Classes and attribute set up
/// </summary>
[AttributeUsage(AttributeTargets.Class |
                AttributeTargets.Struct,
    AllowMultiple = true)
]
public class ComponentNameAttribute : Attribute
{
    private readonly string _name;

    public ComponentNameAttribute(string name)
    {
        _name = name;
    }

    public string GetName()
    {
        return _name;
    }
}

[AttributeUsage(AttributeTargets.Property)]
public class DictionaryKeyAttribute : Attribute
{
    public DictionaryKeyAttribute(string key)
    {
        Key = key;
    }

    public string Key { get; }
}

internal interface IApiModel
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
}

[ComponentName("text_component")]
public record ApiModelA : IApiModel
{
    [DictionaryKey("@id")] public Guid Id { get; set; }

    [DictionaryKey("@name")] public string? Name { get; set; }
}

[ComponentName("text_description_component")]
public record ApiModelB : IApiModel
{
    [DictionaryKey("description")] public string? Description { get; set; }
    [DictionaryKey("@id")] public Guid Id { get; set; }

    [DictionaryKey("@name")] public string? Name { get; set; }
}

public record ApiOutputModel
{
    public string? Name { get; set; }
    public dynamic? ExtraData { get; set; }
}