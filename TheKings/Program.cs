using Newtonsoft.Json;


namespace TheKings
{
    internal record Monarch(int Id, string Name, string City, string House, int StartYear, int EndYear)
    {
        public static Monarch FromDto(MonarchDto dto)
        {
            int startYear;
            int endYear;
            if (!dto.Yrs.Contains('-'))
            {
                startYear = endYear = int.Parse(dto.Yrs);
            }
            else
            {
                var dates = dto.Yrs.Split("-").ToList();
                startYear = int.Parse(dates[0]);
                endYear = string.IsNullOrEmpty(dates[1]) ? DateTime.Now.Year : int.Parse(dates[1]);
            }
            
            return new Monarch(
                Id: dto.Id, 
                Name: dto.Nm, 
                City: dto.Cty, 
                House: dto.Hse, 
                StartYear: startYear,
                EndYear: endYear
            );
        }
    }

    internal record MonarchDto(int Id, string Nm, string Cty, string Hse, string Yrs);
    
    
    class Program
    {
        static async Task<HttpResponseMessage?> GetResponse()
        {
            using HttpClient client = new HttpClient();
            try
            {
                string url = "https://gist.githubusercontent.com/christianpanton/10d65ccef9f29de3acd49d97ed423736/raw/b09563bc0c4b318132c7a738e679d4f984ef0048/kings";
                
                HttpResponseMessage response = await client.GetAsync(url);

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
            }
            return null;
        }
        
        static async Task Main(string[] args)
        {
            var response = await GetResponse();

            if (response is null)
                return;
            
            if (response.IsSuccessStatusCode)
            {
                string jsonString = await response.Content.ReadAsStringAsync();
                
                var dtos = JsonConvert.DeserializeObject<List<MonarchDto>>(jsonString);

                var monarchs = dtos.Select(Monarch.FromDto);

                int numberOfMonarchs = monarchs
                    .Select(x => x.Name)
                    .Distinct()
                    .Count();
                string longestRulingMonarch = monarchs
                    .Select(x => new { Name = x.Name, Ruled = x.EndYear - x.StartYear })
                    .OrderByDescending(e => e.Ruled)
                    .First().Name;

                string longestRulingHouse = monarchs
                    .Select(x => new { Name = x.Name, Ruled = x.EndYear - x.StartYear, House = x.House })
                    .GroupBy(x => x.House)
                    .Select(xl => new { House = xl.First().House, Ruled = xl.Sum(x => x.Ruled) })
                    .OrderByDescending(x => x.Ruled)
                    .First().House;
                string mostCommonFirstName = monarchs
                    .Select(x => new { FirstName = x.Name.Split(" ")[0] })
                    .GroupBy(x => x.FirstName)
                    .Select(xl => new { FirstName = xl.First().FirstName, Count = xl.Count() })
                    .OrderByDescending(x => x.Count)
                    .First().FirstName;
                
                
                Console.WriteLine($"Number of monarchs: {numberOfMonarchs}");
                Console.WriteLine($"Longest ruling monarch: {longestRulingMonarch}");
                Console.WriteLine($"Longest ruling house: {longestRulingHouse}");
                Console.WriteLine($"Most common first name: {mostCommonFirstName}");
            }
            else
            {
                Console.WriteLine($"Error: {response.StatusCode}");
            }
        }
    }
}
