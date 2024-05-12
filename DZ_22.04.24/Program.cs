using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

List<Car> cars = new List<Car>() {
    new Car(Guid.NewGuid().ToString(), "vaz", "granta", "sedan", "benzin", 2, "jac", 7),
new Car(Guid.NewGuid().ToString(), "bmw", "m5", "sedan", "benzin", 6.3, "akpp", 11)
};

/*app.Run(async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";
    var response = context.Response;
    var request = context.Request;
    var path = request.Path;
    string expressionForGuid = @"^/api/cars/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$";

    if (path == "/api/cars" && request.Method == "GET")
    {
        await GetAllCar(response);
    }
    else if (path == "/api/cars" && request.Method == "POST")
    {
        await CreateCar(response, request);
    }
    else if (Regex.IsMatch(path, expressionForGuid) && request.Method == "DELETE")
    {
        string? id = path.Value?.Split("/")[3];
        await DeleteCar(id, response);
    }
    else
    {
        await context.Response.SendFileAsync("HTML/Car.html");
    }
});*/


app.UseWhen(
context => (context.Request.Path == "/api/cars" && context.Request.Method == "GET"),
    appBuilder => appBuilder.Run(async (context) => await GetAllCar(context.Response)));

app.UseWhen(
context => (context.Request.Path == "/api/cars" && context.Request.Method == "POST"),
    appBuilder => appBuilder.Run(async (context) => await CreateCar(context.Response, context.Request)));

app.UseWhen(
context => (Regex.IsMatch(context.Request.Path, @"^/api/cars/\w{8}-\w{4}-\w{4}-\w{4}-\w{12}$") && context.Request.Method == "DELETE"),
    appBuilder =>
    {
        appBuilder.Run(async (context) =>
        {           
            string? id = context.Request.Path.Value?.Split("/")[3];
            await DeleteCar(id, context.Response);
        });     
    }
    );

app.Run(
       async (context) =>
       {
           context.Response.ContentType = "text/html; charset=utf-8";
           await context.Response.SendFileAsync("HTML/Car.html");
       }
   );
app.Run();

async Task GetAllCar(HttpResponse response)
{
    await response.WriteAsJsonAsync(cars);
}

async Task DeleteCar(string? id, HttpResponse response)
{
    Car? car = cars.FirstOrDefault((u) => u.Id == id);
    if (car != null)
    {
        cars.Remove(car);
        await response.WriteAsJsonAsync(car);
    }
    else
    {
        response.StatusCode = 404;
        await response.WriteAsJsonAsync(new { message = "Машина не найден" });
    }
}

async Task CreateCar(HttpResponse response, HttpRequest request)
{
    try
    {
        var jsonoptions = new JsonSerializerOptions();
        jsonoptions.Converters.Add(new CarConverter());
        var car = await request.ReadFromJsonAsync<Car>(jsonoptions);
        if (car != null)
        {
            var c = new Car(Guid.NewGuid().ToString(), car.marka, car.model, car.bodytype, car.enginetype,
                car.engineDisplacement, car.transmissionType, car.averageConsumption);
            cars.Add(c);
            Console.WriteLine(c.Id);
            await response.WriteAsJsonAsync(c);
        }
        else
        {
            throw new Exception("Некорректные данные");
        }
    }
    catch (Exception)
    {
        response.StatusCode = 400;
        await response.WriteAsJsonAsync(new { message = "Некорректные данные" });
    }
}

public record Car(string Id, string marka, string model, string bodytype, string enginetype, double engineDisplacement, string transmissionType, double averageConsumption);

public class CarConverter : JsonConverter<Car>
{
    public override Car Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var Id = Guid.NewGuid().ToString();
        var marka = "Undefined";
        var model = "Undefined";
        var bodytype = "Undefined";
        var enginetype = "Undefined";
        var engineDisplacement = 0.0; ;
        var transmissionType = "Undefined";
        var averageConsumption = 0.0; ;


        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                reader.Read();
                switch (propertyName?.ToLower())
                {
                    case "Id":
                        string? carid = reader.GetString();
                        if (carid != null)
                            Id = carid;
                        break;
                    case "marka":
                        string? carmarka = reader.GetString();
                        if (carmarka != null)
                            marka = carmarka;
                        break;
                    case "model":
                        string? carmodel = reader.GetString();
                        if (carmodel != null)
                            model = carmodel;
                        break;
                    case "bodytype":
                        string? carbodytype = reader.GetString();
                        if (carbodytype != null)
                            bodytype = carbodytype;
                        break;
                    case "enginetype":
                        string? carenginetype = reader.GetString();
                        if (carenginetype != null)
                            enginetype = carenginetype;
                        break;
                    case "enginedisplacement" when reader.TokenType == JsonTokenType.Number:
                        engineDisplacement = reader.GetDouble();
                        break;
                    case "enginedisplacement" when reader.TokenType == JsonTokenType.String:
                        string? stringValue = reader.GetString();
                        if (double.TryParse(stringValue, out double carengineDisplacement))
                        {
                            engineDisplacement = carengineDisplacement;
                        }
                        break;

                    case "transmissiontype":
                        string? cartransmissionType = reader.GetString();
                        if (cartransmissionType != null)
                            transmissionType = cartransmissionType;
                        break;


                    case "averageconsumption" when reader.TokenType == JsonTokenType.Number:
                        averageConsumption = reader.GetDouble();
                        break;
                    case "averageconsumption" when reader.TokenType == JsonTokenType.String:
                        string? str = reader.GetString();
                        if (double.TryParse(str, out double caraverageConsumption))
                        {
                            averageConsumption = caraverageConsumption;
                        }
                        break;

                }
            }
        }
        return new Car(Id, marka, model, bodytype, enginetype, engineDisplacement, transmissionType, averageConsumption);

    }

    public override void Write(Utf8JsonWriter writer, Car car, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("Id", car.Id);
        writer.WriteString("marka", car.marka);
        writer.WriteString("model", car.model);
        writer.WriteString("bodytype", car.bodytype);
        writer.WriteString("enginetype", car.enginetype);
        writer.WriteNumber("engineDisplacement", car.engineDisplacement);
        writer.WriteString("transmissionType", car.transmissionType);
        writer.WriteNumber("averageConsumption", car.averageConsumption);
        writer.WriteEndObject();
    }
}