using Andy.TUI.Terminal;
using System.Diagnostics;

namespace Andy.TUI.Examples.Terminal;

/// <summary>
/// Demonstrates a weather dashboard with animated weather icons, temperature graphs, and forecast data.
/// </summary>
public class WeatherDashboardExample
{
    private enum WeatherCondition
    {
        Sunny,
        PartlyCloudy,
        Cloudy,
        Rainy,
        Stormy,
        Snowy,
        Foggy
    }

    private class WeatherData
    {
        public string City { get; set; } = "";
        public int Temperature { get; set; }
        public int Humidity { get; set; }
        public int WindSpeed { get; set; }
        public string WindDirection { get; set; } = "";
        public WeatherCondition Condition { get; set; }
        public int Pressure { get; set; }
        public int UVIndex { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    private class ForecastDay
    {
        public string Day { get; set; } = "";
        public int HighTemp { get; set; }
        public int LowTemp { get; set; }
        public WeatherCondition Condition { get; set; }
        public int ChanceOfRain { get; set; }
    }

    public static void Run()
    {
        Console.WriteLine("=== Weather Dashboard ===");
        Console.WriteLine("Live weather information with animated displays");
        Console.WriteLine("Loading weather data...");
        Thread.Sleep(1500);

        var terminal = new AnsiTerminal();
        using var renderingSystem = new RenderingSystem(terminal);
        renderingSystem.Initialize();

        // Hide cursor
        terminal.CursorVisible = false;

        // Create input handler for exit
        var inputHandler = new ConsoleInputHandler();
        bool exit = false;
        var pressedKeys = new HashSet<ConsoleKey>();

        inputHandler.KeyPressed += (_, e) =>
        {
            if (e.Key == ConsoleKey.Escape || e.Key == ConsoleKey.Q)
                exit = true;
            else
                pressedKeys.Add(e.Key);
        };
        inputHandler.Start();

        // Initialize weather data (simulated)
        var currentWeather = GenerateCurrentWeather();
        var forecast = GenerateForecast();
        var temperatureHistory = GenerateTemperatureHistory();

        int selectedCity = 0;
        var cities = new[] { "New York", "London", "Tokyo", "Sydney", "Paris" };

        var frameCount = 0;
        var startTime = DateTime.Now;

        // Configure render scheduler
        renderingSystem.Scheduler.TargetFps = 20;

        // Animation render function
        Action? renderFrame = null;
        renderFrame = () =>
        {
            if (exit)
                return;

            renderingSystem.Clear();

            // Handle input
            if (pressedKeys.Contains(ConsoleKey.LeftArrow))
            {
                selectedCity = (selectedCity - 1 + cities.Length) % cities.Length;
                currentWeather = GenerateCurrentWeatherForCity(cities[selectedCity]);
                forecast = GenerateForecast();
                pressedKeys.Remove(ConsoleKey.LeftArrow);
            }
            if (pressedKeys.Contains(ConsoleKey.RightArrow))
            {
                selectedCity = (selectedCity + 1) % cities.Length;
                currentWeather = GenerateCurrentWeatherForCity(cities[selectedCity]);
                forecast = GenerateForecast();
                pressedKeys.Remove(ConsoleKey.RightArrow);
            }

            // Update weather data periodically
            if (frameCount % 200 == 0) // Every ~10 seconds at 20 FPS
            {
                currentWeather = GenerateCurrentWeatherForCity(cities[selectedCity]);
                temperatureHistory = GenerateTemperatureHistory();
            }

            // Draw dashboard
            DrawWeatherDashboard(renderingSystem, currentWeather, forecast, temperatureHistory,
                               frameCount, startTime, selectedCity, cities.Length);

            frameCount++;

            // Queue next frame
            renderingSystem.Scheduler.QueueRender(renderFrame);
        };

        // Start animation
        renderingSystem.Scheduler.QueueRender(renderFrame);

        // Wait for exit
        while (!exit)
        {
            Thread.Sleep(50);
        }

        inputHandler.Stop();
        inputHandler.Dispose();

        // Restore cursor
        terminal.CursorVisible = true;

        Console.Clear();
        Console.WriteLine("\nWeather dashboard closed. Stay safe out there!");
    }

    private static void DrawWeatherDashboard(RenderingSystem renderingSystem, WeatherData weather,
                                           List<ForecastDay> forecast, List<int> temperatureHistory,
                                           int frameCount, DateTime startTime, int selectedCity, int totalCities)
    {
        // Draw title
        var titleStyle = Style.Default.WithForegroundColor(Color.Yellow).WithBold();
        renderingSystem.WriteText(2, 1, "🌤️  Weather Dashboard", titleStyle);

        // Draw city navigation
        var navStyle = Style.Default.WithForegroundColor(Color.Cyan);
        renderingSystem.WriteText(2, 2, $"City {selectedCity + 1}/{totalCities}: {weather.City}", navStyle);

        // Draw current weather section
        DrawCurrentWeather(renderingSystem, weather, frameCount, 2, 4);

        // Draw forecast section
        DrawForecast(renderingSystem, forecast, 2, 15);

        // Draw temperature graph
        DrawTemperatureGraph(renderingSystem, temperatureHistory, renderingSystem.Terminal.Width - 40, 4);

        // Draw additional info
        DrawAdditionalInfo(renderingSystem, weather, renderingSystem.Terminal.Width - 25, 15);

        // Draw controls
        var controlsStyle = Style.Default.WithForegroundColor(Color.DarkGray);
        renderingSystem.WriteText(2, renderingSystem.Terminal.Height - 2, "← → Change City | ESC/Q Exit", controlsStyle);

        // Draw performance stats and last updated
        var elapsed = (DateTime.Now - startTime).TotalSeconds;
        var fps = frameCount / elapsed;
        var statsStyle = Style.Default.WithForegroundColor(Color.Green);
        renderingSystem.WriteText(renderingSystem.Terminal.Width - 15, 1, $"FPS: {fps:F1}", statsStyle);

        var updateStyle = Style.Default.WithForegroundColor(Color.Gray);
        renderingSystem.WriteText(renderingSystem.Terminal.Width - 30, 2, $"Updated: {weather.LastUpdated:HH:mm:ss}", updateStyle);
    }

    private static void DrawCurrentWeather(RenderingSystem renderingSystem, WeatherData weather, int frameCount, int x, int y)
    {
        // Draw weather icon (animated)
        DrawWeatherIcon(renderingSystem, weather.Condition, x, y, frameCount);

        // Draw temperature
        var tempStyle = Style.Default.WithForegroundColor(GetTemperatureColor(weather.Temperature)).WithBold();
        renderingSystem.WriteText(x + 12, y, $"{weather.Temperature}°C", tempStyle);

        // Draw condition
        var conditionStyle = Style.Default.WithForegroundColor(Color.White);
        renderingSystem.WriteText(x + 12, y + 1, GetConditionText(weather.Condition), conditionStyle);

        // Draw additional metrics
        var metricStyle = Style.Default.WithForegroundColor(Color.Cyan);
        renderingSystem.WriteText(x, y + 8, $"Humidity: {weather.Humidity}%", metricStyle);
        renderingSystem.WriteText(x, y + 9, $"Wind: {weather.WindSpeed} km/h {weather.WindDirection}", metricStyle);
        renderingSystem.WriteText(x + 25, y + 8, $"Pressure: {weather.Pressure} hPa", metricStyle);
        renderingSystem.WriteText(x + 25, y + 9, $"UV Index: {weather.UVIndex}", metricStyle);
    }

    private static void DrawWeatherIcon(RenderingSystem renderingSystem, WeatherCondition condition, int x, int y, int frame)
    {
        var iconStyle = Style.Default.WithForegroundColor(GetConditionColor(condition));
        var secondaryStyle = Style.Default.WithForegroundColor(Color.White);

        switch (condition)
        {
            case WeatherCondition.Sunny:
                DrawSunIcon(renderingSystem, x, y, frame, iconStyle);
                break;
            case WeatherCondition.PartlyCloudy:
                DrawPartlyCloudyIcon(renderingSystem, x, y, frame, iconStyle, secondaryStyle);
                break;
            case WeatherCondition.Cloudy:
                DrawCloudIcon(renderingSystem, x, y, frame, iconStyle);
                break;
            case WeatherCondition.Rainy:
                DrawRainIcon(renderingSystem, x, y, frame, iconStyle, secondaryStyle);
                break;
            case WeatherCondition.Stormy:
                DrawStormIcon(renderingSystem, x, y, frame, iconStyle, secondaryStyle);
                break;
            case WeatherCondition.Snowy:
                DrawSnowIcon(renderingSystem, x, y, frame, iconStyle);
                break;
            case WeatherCondition.Foggy:
                DrawFogIcon(renderingSystem, x, y, frame, iconStyle);
                break;
        }
    }

    private static void DrawSunIcon(RenderingSystem renderingSystem, int x, int y, int frame, Style style)
    {
        // Sun rays (animated)
        var rayPositions = new[]
        {
            (x + 4, y - 1), (x + 6, y - 1), (x + 8, y + 1),
            (x + 8, y + 3), (x + 6, y + 5), (x + 4, y + 5),
            (x + 2, y + 3), (x + 2, y + 1)
        };

        var rayIndex = (frame / 10) % rayPositions.Length;
        for (int i = 0; i < rayPositions.Length; i++)
        {
            var intensity = i == rayIndex ? 1.0 : 0.5;
            var rayColor = Color.FromRgb((byte)(255 * intensity), (byte)(255 * intensity), 0);
            var rayStyle = Style.Default.WithForegroundColor(rayColor);
            renderingSystem.Buffer.SetCell(rayPositions[i].Item1, rayPositions[i].Item2, '*', rayStyle);
        }

        // Sun body
        renderingSystem.WriteText(x + 3, y + 1, "┌───┐", style);
        renderingSystem.WriteText(x + 3, y + 2, "│ ☀ │", style);
        renderingSystem.WriteText(x + 3, y + 3, "└───┘", style);
    }

    private static void DrawPartlyCloudyIcon(RenderingSystem renderingSystem, int x, int y, int frame, Style sunStyle, Style cloudStyle)
    {
        // Sun (smaller)
        renderingSystem.WriteText(x, y, "☀", sunStyle);

        // Cloud
        renderingSystem.WriteText(x + 2, y + 1, "☁☁☁", cloudStyle);
        renderingSystem.WriteText(x + 1, y + 2, "☁☁☁☁", cloudStyle);
    }

    private static void DrawCloudIcon(RenderingSystem renderingSystem, int x, int y, int frame, Style style)
    {
        renderingSystem.WriteText(x + 1, y, "☁☁☁", style);
        renderingSystem.WriteText(x, y + 1, "☁☁☁☁☁", style);
        renderingSystem.WriteText(x, y + 2, "☁☁☁☁☁", style);
    }

    private static void DrawRainIcon(RenderingSystem renderingSystem, int x, int y, int frame, Style cloudStyle, Style rainStyle)
    {
        // Cloud
        renderingSystem.WriteText(x + 1, y, "☁☁☁", cloudStyle);
        renderingSystem.WriteText(x, y + 1, "☁☁☁☁☁", cloudStyle);

        // Animated rain drops
        var rainY = y + 2 + (frame / 5) % 3;
        for (int i = 0; i < 5; i++)
        {
            if (rainY + i < y + 6)
                renderingSystem.Buffer.SetCell(x + i + 1, rainY + (i % 2), '|', rainStyle);
        }
    }

    private static void DrawStormIcon(RenderingSystem renderingSystem, int x, int y, int frame, Style cloudStyle, Style lightningStyle)
    {
        // Dark cloud
        var darkStyle = Style.Default.WithForegroundColor(Color.DarkGray);
        renderingSystem.WriteText(x + 1, y, "☁☁☁", darkStyle);
        renderingSystem.WriteText(x, y + 1, "☁☁☁☁☁", darkStyle);

        // Lightning (flashing)
        if ((frame / 8) % 4 == 0)
        {
            renderingSystem.WriteText(x + 2, y + 2, "⚡", lightningStyle);
        }
    }

    private static void DrawSnowIcon(RenderingSystem renderingSystem, int x, int y, int frame, Style style)
    {
        // Cloud
        renderingSystem.WriteText(x + 1, y, "☁☁☁", style);
        renderingSystem.WriteText(x, y + 1, "☁☁☁☁☁", style);

        // Animated snowflakes
        var snowflakes = new[] { '❄', '❅', '✻' };
        for (int i = 0; i < 6; i++)
        {
            var snowY = y + 2 + ((frame + i * 7) / 8) % 4;
            var flakeIndex = (frame + i) % snowflakes.Length;
            renderingSystem.Buffer.SetCell(x + i, snowY, snowflakes[flakeIndex], style);
        }
    }

    private static void DrawFogIcon(RenderingSystem renderingSystem, int x, int y, int frame, Style style)
    {
        var fogStyle = Style.Default.WithForegroundColor(Color.Gray);
        for (int i = 0; i < 4; i++)
        {
            var opacity = (Math.Sin((frame * 0.1) + i) + 1) / 2;
            var alpha = (byte)(opacity * 150 + 50);
            var dynamicStyle = Style.Default.WithForegroundColor(Color.FromRgb(alpha, alpha, alpha));
            renderingSystem.WriteText(x, y + i, "≈≈≈≈≈≈", dynamicStyle);
        }
    }

    private static void DrawForecast(RenderingSystem renderingSystem, List<ForecastDay> forecast, int x, int y)
    {
        var headerStyle = Style.Default.WithForegroundColor(Color.Yellow).WithBold();
        renderingSystem.WriteText(x, y, "5-Day Forecast", headerStyle);

        for (int i = 0; i < forecast.Count; i++)
        {
            var day = forecast[i];
            var dayStyle = Style.Default.WithForegroundColor(Color.White);
            var tempStyle = Style.Default.WithForegroundColor(GetTemperatureColor(day.HighTemp));
            var rainStyle = Style.Default.WithForegroundColor(Color.Blue);

            int dayX = x + i * 15;
            renderingSystem.WriteText(dayX, y + 2, day.Day, dayStyle);
            renderingSystem.WriteText(dayX, y + 3, GetConditionIcon(day.Condition), Style.Default.WithForegroundColor(GetConditionColor(day.Condition)));
            renderingSystem.WriteText(dayX, y + 4, $"{day.HighTemp}°/{day.LowTemp}°", tempStyle);
            renderingSystem.WriteText(dayX, y + 5, $"{day.ChanceOfRain}% rain", rainStyle);
        }
    }

    private static void DrawTemperatureGraph(RenderingSystem renderingSystem, List<int> temperatures, int x, int y)
    {
        var headerStyle = Style.Default.WithForegroundColor(Color.Yellow).WithBold();
        renderingSystem.WriteText(x, y, "24h Temperature", headerStyle);

        var graphHeight = 8;
        var graphWidth = Math.Min(temperatures.Count, 30);

        var minTemp = temperatures.Min();
        var maxTemp = temperatures.Max();
        var tempRange = Math.Max(1, maxTemp - minTemp);

        // Draw graph
        for (int i = 0; i < graphWidth; i++)
        {
            var temp = temperatures[i];
            var normalizedHeight = (int)((temp - minTemp) * graphHeight / tempRange);
            var color = GetTemperatureColor(temp);
            var style = Style.Default.WithForegroundColor(color);

            // Draw bar
            for (int h = 0; h <= normalizedHeight; h++)
            {
                renderingSystem.Buffer.SetCell(x + i, y + graphHeight + 2 - h, '█', style);
            }

            // Draw temperature value at top
            if (i % 4 == 0)
            {
                renderingSystem.WriteText(x + i, y + 1, $"{temp}°", Style.Default.WithForegroundColor(Color.Cyan));
            }
        }

        // Draw axis
        var axisStyle = Style.Default.WithForegroundColor(Color.DarkGray);
        for (int i = 0; i < graphWidth; i++)
        {
            renderingSystem.Buffer.SetCell(x + i, y + graphHeight + 3, '─', axisStyle);
        }
    }

    private static void DrawAdditionalInfo(RenderingSystem renderingSystem, WeatherData weather, int x, int y)
    {
        var headerStyle = Style.Default.WithForegroundColor(Color.Yellow).WithBold();
        var infoStyle = Style.Default.WithForegroundColor(Color.White);

        renderingSystem.WriteText(x, y, "Details", headerStyle);

        // Air quality indicator
        var aqiColor = weather.UVIndex < 3 ? Color.Green : weather.UVIndex < 6 ? Color.Yellow : Color.Red;
        var aqiStyle = Style.Default.WithForegroundColor(aqiColor);
        renderingSystem.WriteText(x, y + 2, "Air Quality:", infoStyle);
        var aqiText = weather.UVIndex < 3 ? "Good" : weather.UVIndex < 6 ? "Moderate" : "Poor";
        renderingSystem.WriteText(x, y + 3, aqiText, aqiStyle);

        // Comfort index
        var comfort = CalculateComfortIndex(weather.Temperature, weather.Humidity);
        var comfortColor = comfort > 70 ? Color.Green : comfort > 40 ? Color.Yellow : Color.Red;
        var comfortStyle = Style.Default.WithForegroundColor(comfortColor);
        renderingSystem.WriteText(x, y + 5, "Comfort:", infoStyle);
        renderingSystem.WriteText(x, y + 6, $"{comfort}%", comfortStyle);
    }

    private static WeatherData GenerateCurrentWeather()
    {
        return GenerateCurrentWeatherForCity("New York");
    }

    private static WeatherData GenerateCurrentWeatherForCity(string city)
    {
        var random = new Random();
        var conditions = Enum.GetValues<WeatherCondition>();

        return new WeatherData
        {
            City = city,
            Temperature = random.Next(-10, 35),
            Humidity = random.Next(30, 90),
            WindSpeed = random.Next(0, 25),
            WindDirection = new[] { "N", "NE", "E", "SE", "S", "SW", "W", "NW" }[random.Next(8)],
            Condition = conditions[random.Next(conditions.Length)],
            Pressure = random.Next(980, 1030),
            UVIndex = random.Next(0, 11),
            LastUpdated = DateTime.Now
        };
    }

    private static List<ForecastDay> GenerateForecast()
    {
        var random = new Random();
        var conditions = Enum.GetValues<WeatherCondition>();
        var days = new[] { "Today", "Tomorrow", "Thursday", "Friday", "Saturday" };

        return days.Select(day => new ForecastDay
        {
            Day = day,
            HighTemp = random.Next(15, 30),
            LowTemp = random.Next(-5, 15),
            Condition = conditions[random.Next(conditions.Length)],
            ChanceOfRain = random.Next(0, 100)
        }).ToList();
    }

    private static List<int> GenerateTemperatureHistory()
    {
        var random = new Random();
        var temps = new List<int>();
        var baseTemp = random.Next(10, 25);

        for (int i = 0; i < 24; i++)
        {
            var variation = (int)(Math.Sin(i * Math.PI / 12) * 8) + random.Next(-3, 4);
            temps.Add(baseTemp + variation);
        }

        return temps;
    }

    private static Color GetTemperatureColor(int temperature)
    {
        if (temperature < 0) return Color.FromRgb(100, 150, 255); // Cold blue
        if (temperature < 10) return Color.FromRgb(150, 200, 255); // Cool blue
        if (temperature < 20) return Color.Green; // Comfortable green
        if (temperature < 30) return Color.Yellow; // Warm yellow
        return Color.Red; // Hot red
    }

    private static Color GetConditionColor(WeatherCondition condition)
    {
        return condition switch
        {
            WeatherCondition.Sunny => Color.Yellow,
            WeatherCondition.PartlyCloudy => Color.FromRgb(255, 255, 150),
            WeatherCondition.Cloudy => Color.Gray,
            WeatherCondition.Rainy => Color.Blue,
            WeatherCondition.Stormy => Color.FromRgb(100, 0, 100),
            WeatherCondition.Snowy => Color.White,
            WeatherCondition.Foggy => Color.DarkGray,
            _ => Color.White
        };
    }

    private static string GetConditionText(WeatherCondition condition)
    {
        return condition switch
        {
            WeatherCondition.Sunny => "Sunny",
            WeatherCondition.PartlyCloudy => "Partly Cloudy",
            WeatherCondition.Cloudy => "Cloudy",
            WeatherCondition.Rainy => "Rainy",
            WeatherCondition.Stormy => "Stormy",
            WeatherCondition.Snowy => "Snowy",
            WeatherCondition.Foggy => "Foggy",
            _ => "Unknown"
        };
    }

    private static string GetConditionIcon(WeatherCondition condition)
    {
        return condition switch
        {
            WeatherCondition.Sunny => "☀",
            WeatherCondition.PartlyCloudy => "⛅",
            WeatherCondition.Cloudy => "☁",
            WeatherCondition.Rainy => "🌧",
            WeatherCondition.Stormy => "⛈",
            WeatherCondition.Snowy => "❄",
            WeatherCondition.Foggy => "🌫",
            _ => "?"
        };
    }

    private static int CalculateComfortIndex(int temperature, int humidity)
    {
        // Simplified comfort calculation
        var tempComfort = temperature >= 18 && temperature <= 24 ? 100 : Math.Max(0, 100 - Math.Abs(21 - temperature) * 5);
        var humidityComfort = humidity >= 40 && humidity <= 60 ? 100 : Math.Max(0, 100 - Math.Abs(50 - humidity) * 2);
        return (tempComfort + humidityComfort) / 2;
    }
}