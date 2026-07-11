using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Collections.Generic;

public class Program
{
    private sealed class ExternalLoginResponse
    {
        [JsonPropertyName(""Status_Code"")]
        public int StatusCode { get; set; }

        [JsonPropertyName(""Response_Body"")]
        public List<ExternalLoginResponseBody>? ResponseBody { get; set; }
    }

    private sealed class ExternalLoginResponseBody
    {
        [JsonPropertyName(""Email"")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName(""User_Locations"")]
        public List<ExternalLocation>? UserLocations { get; set; }
    }

    private sealed class ExternalLocation
    {
        [JsonPropertyName(""Location_Code"")]
        public string LocationCode { get; set; } = string.Empty;

        [JsonPropertyName(""Location_Name"")]
        public string LocationName { get; set; } = string.Empty;
    }

    public static void Main()
    {
        var json = @""{""""Status_Code"""":200,""""Sync_Time"""":"""""""",""""Message"""":""""GetLoginData POS API Executed Successfully."""",""""Response_Body"""":[{""""User_Code"""":""""EZCMP1/EZUSR-1"""",""""User_Display_Name"""":""""eZuite Admin"""",""""Email"""":""""info@enhanzer.com"""",""""User_Employee_Code"""":""""EZCMP1/EZLOC2/EMP-7"""",""""Company_Code"""":""""EZCMP-1"""",""""User_Locations"""":[{""""Location_Code"""":""""EZCMP1/EZLOC-9"""",""""Location_Name"""":""""Lorry 1""""}]}]}@"";
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        var response = JsonSerializer.Deserialize<ExternalLoginResponse>(json, options);
        if (response != null && response.ResponseBody != null && response.ResponseBody.Count > 0)
        {
            var user = response.ResponseBody[0];
            Console.WriteLine($""Email: {user.Email}"");
            if (user.UserLocations != null)
                Console.WriteLine($""Locations: {user.UserLocations.Count}"");
        }
        else
        {
            Console.WriteLine(""Response is null or empty"");
        }
    }
}
