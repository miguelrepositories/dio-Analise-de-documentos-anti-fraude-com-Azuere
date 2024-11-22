using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    private static readonly string subscriptionKey = "chave";
    private static readonly string endpoint = "endpoint";
    private static readonly string formRecognizerEndpoint = $"{endpoint}/formrecognizer/v2.1/layout/analyze";

    static async Task Main(string[] args)
    {
        // Caminho do documento
        string filePath = "caminho/do/seu/documento.pdf"; 

        if (File.Exists(filePath))
        {
            byte[] fileData = await File.ReadAllBytesAsync(filePath);
            string result = await AnalyzeDocument(fileData);
            
            bool isFraud = AnalyzeForFraud(result);
            if (isFraud)
            {
                Console.WriteLine("Fraude detectada no documento.");
            }
            else
            {
                Console.WriteLine("Nenhuma fraude detectada.");
            }
        }
        else
        {
            Console.WriteLine("Arquivo não encontrado.");
        }
    }

    static async Task<string> AnalyzeDocument(byte[] documentData)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            using (ByteArrayContent content = new ByteArrayContent(documentData))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf"); // Ou "image/jpeg" para imagens
                HttpResponseMessage response = await client.PostAsync(formRecognizerEndpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Análise realizada com sucesso.");
                    return jsonResponse;
                }
                else
                {
                    Console.WriteLine($"Erro na análise: {response.StatusCode}");
                    return null;
                }
            }
        }
    }

    static bool AnalyzeForFraud(string jsonData)
    {
        if (string.IsNullOrEmpty(jsonData))
            return false;

        var document = JsonDocument.Parse(jsonData);
        

        foreach (var element in document.RootElement.EnumerateObject())
        {
            if (element.Name.Contains("fraud", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        
        return false;
    }
}
