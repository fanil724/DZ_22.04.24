using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Run(async (context) =>
{
    context.Response.ContentType = "text/html; charset=utf-8";


    var response = context.Response;
    var request = context.Request;
    if (request.Path == "/postcar")
    {
        var message = "Некорректные данные";

        if (request.HasJsonContentType())
        {
            var jsonoptions = new JsonSerializerOptions();
            jsonoptions.Converters.Add(new CarConverter());              
            var car = await request.ReadFromJsonAsync<Car>(jsonoptions);
            if (car != null)
            {
                message = $"Marka: {car.marka};  Model: {car.model}; Bodytype: {car.bodytype};  Enginetype: {car.enginetype};" +
                $" EngineDisplacement: {car.engineDisplacement};  TransmissionType: {car.transmissionType};" +
                $" AverageConsumption: {car.averageConsumption}; ";
            }
        }
        await response.WriteAsJsonAsync(new { text = message });
    }
    else
    {
        await context.Response.SendFileAsync("HTML/Car.html");
    }
});

app.Run();




public record Car(string marka, string model, string bodytype, string enginetype, double engineDisplacement, string transmissionType, double averageConsumption);



public class CarConverter : JsonConverter<Car>
{
    public override Car Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
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
        return new Car( marka,  model,  bodytype,  enginetype,  engineDisplacement,  transmissionType,  averageConsumption);

    }

    public override void Write(Utf8JsonWriter writer, Car car, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
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

