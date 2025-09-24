using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Text.Json;

namespace PrimeNumberCalculator
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Prime Number Calculator - Web Interface Starting...");
            
            var builder = WebApplication.CreateBuilder(args);
            
            // Add services
            builder.Services.AddCors(options =>
            {
                options.AddDefaultPolicy(policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline
            app.UseCors();
            app.UseStaticFiles();

            // API endpoint for prime number calculation
            app.MapPost("/api/check-prime", async (HttpContext context) =>
            {
                try
                {
                    var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
                    var request = JsonSerializer.Deserialize<PrimeRequest>(requestBody);
                    
                    if (request?.Number == null)
                    {
                        return Results.BadRequest(new { error = "Invalid number provided" });
                    }

                    var number = request.Number.Value;
                    var isPrime = IsPrime(number);
                    var factors = isPrime ? new long[0] : GetFactors(number);
                    var explanation = GetExplanation(number, isPrime, factors);

                    var response = new PrimeResponse
                    {
                        Number = number,
                        IsPrime = isPrime,
                        Factors = factors,
                        Explanation = explanation,
                        ProcessingTime = DateTime.UtcNow
                    };

                    Console.WriteLine($"Checked {number}: {(isPrime ? "PRIME" : "NOT PRIME")}");
                    
                    return Results.Ok(response);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                    return Results.BadRequest(new { error = "Invalid request format" });
                }
            });

            // Serve the main HTML page
            app.MapGet("/", async context =>
            {
                var htmlContent = GetHtmlContent();
                context.Response.ContentType = "text/html";
                await context.Response.WriteAsync(htmlContent);
            });

            Console.WriteLine("Prime Number Calculator running at http://localhost:8080");
            Console.WriteLine("Press Ctrl+C to stop the application");
            
            app.Run("http://0.0.0.0:8080");
        }

        public static bool IsPrime(long number)
        {
            if (number <= 1) return false;
            if (number == 2) return true;
            if (number % 2 == 0) return false;

            var sqrt = (long)Math.Sqrt(number);
            for (long i = 3; i <= sqrt; i += 2)
            {
                if (number % i == 0)
                    return false;
            }
            
            return true;
        }

        public static long[] GetFactors(long number)
        {
            if (number <= 1) return new long[0];
            
            var factors = new System.Collections.Generic.List<long>();
            
            for (long i = 2; i <= Math.Sqrt(number); i++)
            {
                if (number % i == 0)
                {
                    factors.Add(i);
                    if (i != number / i)
                        factors.Add(number / i);
                }
            }
            
            factors.Sort();
            return factors.ToArray();
        }

        public static string GetExplanation(long number, bool isPrime, long[] factors)
        {
            if (number <= 1)
                return $"{number} is not considered a prime number. Prime numbers are natural numbers greater than 1.";
            
            if (number == 2)
                return "2 is the only even prime number. All other even numbers are divisible by 2.";
            
            if (isPrime)
                return $"{number} is a prime number! It has no positive divisors other than 1 and itself.";
            
            var factorList = string.Join(", ", factors);
            return $"{number} is not a prime number. It can be divided by: {factorList}";
        }

        public static string GetHtmlContent()
        {
            return @"<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Prime Number Calculator</title>
    <link href=""https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;500;700&display=swap"" rel=""stylesheet"">
    <link href=""https://fonts.googleapis.com/icon?family=Material+Icons"" rel=""stylesheet"">
    <style>
        :root {
            --primary-color: #1976d2;
            --primary-dark: #1565c0;
            --primary-light: #42a5f5;
            --secondary-color: #dc004e;
            --success-color: #4caf50;
            --error-color: #f44336;
            --warning-color: #ff9800;
            --surface-color: #ffffff;
            --background-color: #fafafa;
            --on-surface: #212121;
            --on-surface-variant: #757575;
        }

        * {
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }

        body {
            font-family: 'Roboto', sans-serif;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 24px;
        }

        .container {
            max-width: 600px;
            width: 100%;
        }

        .card {
            background: var(--surface-color);
            border-radius: 16px;
            box-shadow: 
                0px 2px 4px rgba(0,0,0,0.14),
                0px 3px 4px rgba(0,0,0,0.12),
                0px 1px 5px rgba(0,0,0,0.20);
            overflow: hidden;
            transition: box-shadow 0.3s cubic-bezier(0.4, 0.0, 0.2, 1);
            animation: slideUp 0.6s cubic-bezier(0.4, 0.0, 0.2, 1);
        }

        .card:hover {
            box-shadow: 
                0px 5px 5px rgba(0,0,0,0.14),
                0px 9px 9px rgba(0,0,0,0.12),
                0px 3px 16px rgba(0,0,0,0.20);
        }

        @keyframes slideUp {
            from {
                opacity: 0;
                transform: translateY(40px);
            }
            to {
                opacity: 1;
                transform: translateY(0);
            }
        }

        .card-header {
            background: var(--primary-color);
            color: white;
            padding: 32px;
            text-align: center;
            position: relative;
            overflow: hidden;
        }

        .card-header::before {
            content: '';
            position: absolute;
            top: -50%;
            left: -50%;
            width: 200%;
            height: 200%;
            background: linear-gradient(45deg, transparent, rgba(255,255,255,0.1), transparent);
            transform: rotate(45deg);
            animation: shimmer 3s infinite;
        }

        @keyframes shimmer {
            0% { transform: translateX(-100%) translateY(-100%) rotate(45deg); }
            100% { transform: translateX(100%) translateY(100%) rotate(45deg); }
        }

        .card-title {
            font-size: 2rem;
            font-weight: 500;
            margin-bottom: 8px;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 12px;
        }

        .card-subtitle {
            font-size: 1rem;
            opacity: 0.9;
            font-weight: 300;
        }

        .card-content {
            padding: 32px;
        }

        .input-container {
            position: relative;
            margin-bottom: 24px;
        }

        .text-field {
            position: relative;
            display: flex;
            flex-direction: column;
        }

        .text-input {
            width: 100%;
            padding: 16px 16px 16px 16px;
            border: 2px solid #e0e0e0;
            border-radius: 8px;
            font-size: 1.1rem;
            font-family: 'Roboto', sans-serif;
            background: transparent;
            outline: none;
            transition: all 0.3s cubic-bezier(0.4, 0.0, 0.2, 1);
        }

        .text-input:focus {
            border-color: var(--primary-color);
            box-shadow: 0 0 0 2px rgba(25, 118, 210, 0.12);
        }

        .text-input:invalid {
            border-color: var(--error-color);
        }

        .input-label {
            position: absolute;
            top: 50%;
            left: 16px;
            transform: translateY(-50%);
            background: white;
            padding: 0 8px;
            color: var(--on-surface-variant);
            font-size: 1rem;
            transition: all 0.3s cubic-bezier(0.4, 0.0, 0.2, 1);
            pointer-events: none;
        }

        .text-input:focus + .input-label,
        .text-input:not(:placeholder-shown) + .input-label {
            top: 0;
            font-size: 0.85rem;
            color: var(--primary-color);
            font-weight: 500;
        }

        .btn {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
            padding: 16px 32px;
            border: none;
            border-radius: 8px;
            font-size: 1rem;
            font-weight: 500;
            font-family: 'Roboto', sans-serif;
            cursor: pointer;
            transition: all 0.3s cubic-bezier(0.4, 0.0, 0.2, 1);
            position: relative;
            overflow: hidden;
            text-transform: uppercase;
            letter-spacing: 0.5px;
        }

        .btn:disabled {
            opacity: 0.6;
            cursor: not-allowed;
        }

        .btn::before {
            content: '';
            position: absolute;
            top: 50%;
            left: 50%;
            width: 0;
            height: 0;
            background: rgba(255, 255, 255, 0.2);
            border-radius: 50%;
            transform: translate(-50%, -50%);
            transition: width 0.3s, height 0.3s;
        }

        .btn:active::before {
            width: 300px;
            height: 300px;
        }

        .btn-primary {
            background: var(--primary-color);
            color: white;
            width: 100%;
        }

        .btn-primary:hover:not(:disabled) {
            background: var(--primary-dark);
            box-shadow: 0px 2px 4px rgba(0,0,0,0.14), 0px 4px 5px rgba(0,0,0,0.12), 0px 1px 10px rgba(0,0,0,0.20);
        }

        .result-container {
            margin-top: 24px;
            opacity: 0;
            transform: translateY(20px);
            transition: all 0.5s cubic-bezier(0.4, 0.0, 0.2, 1);
        }

        .result-container.show {
            opacity: 1;
            transform: translateY(0);
        }

        .result-card {
            padding: 24px;
            border-radius: 12px;
            margin-bottom: 16px;
            animation: fadeInScale 0.5s cubic-bezier(0.4, 0.0, 0.2, 1);
        }

        @keyframes fadeInScale {
            from {
                opacity: 0;
                transform: scale(0.9);
            }
            to {
                opacity: 1;
                transform: scale(1);
            }
        }

        .result-prime {
            background: linear-gradient(135deg, #4caf50, #45a049);
            color: white;
        }

        .result-not-prime {
            background: linear-gradient(135deg, #ff5722, #e64a19);
            color: white;
        }

        .result-title {
            font-size: 1.5rem;
            font-weight: 500;
            margin-bottom: 12px;
            display: flex;
            align-items: center;
            gap: 12px;
        }

        .result-explanation {
            font-size: 1rem;
            line-height: 1.6;
            margin-bottom: 16px;
        }

        .factors-container {
            background: rgba(255, 255, 255, 0.1);
            padding: 16px;
            border-radius: 8px;
            margin-top: 16px;
        }

        .factors-title {
            font-size: 1rem;
            font-weight: 500;
            margin-bottom: 8px;
        }

        .factors-list {
            display: flex;
            flex-wrap: wrap;
            gap: 8px;
        }

        .factor-chip {
            background: rgba(255, 255, 255, 0.2);
            color: white;
            padding: 4px 12px;
            border-radius: 16px;
            font-size: 0.9rem;
            font-weight: 500;
        }

        .loading {
            display: none;
            justify-content: center;
            align-items: center;
            margin: 16px 0;
        }

        .loading.show {
            display: flex;
        }

        .spinner {
            width: 24px;
            height: 24px;
            border: 3px solid rgba(25, 118, 210, 0.3);
            border-top: 3px solid var(--primary-color);
            border-radius: 50%;
            animation: spin 1s linear infinite;
        }

        @keyframes spin {
            0% { transform: rotate(0deg); }
            100% { transform: rotate(360deg); }
        }

        .error-message {
            color: var(--error-color);
            font-size: 0.9rem;
            margin-top: 8px;
            display: none;
        }

        .error-message.show {
            display: block;
        }

        @media (max-width: 480px) {
            .card-header {
                padding: 24px 16px;
            }
            
            .card-content {
                padding: 24px 16px;
            }
            
            .card-title {
                font-size: 1.5rem;
            }
        }
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""card"">
            <div class=""card-header"">
                <h1 class=""card-title"">
                    <i class=""material-icons"" style=""font-size: 2rem;"">calculate</i>
                    Prime Number Calculator
                </h1>
                <p class=""card-subtitle"">Discover if your number is prime with detailed analysis</p>
            </div>
            
            <div class=""card-content"">
                <form id=""primeForm"">
                    <div class=""input-container"">
                        <div class=""text-field"">
                            <input 
                                type=""number"" 
                                id=""numberInput"" 
                                class=""text-input"" 
                                placeholder="" ""
                                min=""1""
                                max=""9223372036854775807""
                                required
                            >
                            <label for=""numberInput"" class=""input-label"">Enter a number</label>
                        </div>
                        <div id=""errorMessage"" class=""error-message"">Please enter a valid positive number</div>
                    </div>
                    
                    <button type=""submit"" id=""submitBtn"" class=""btn btn-primary"">
                        <i class=""material-icons"">search</i>
                        Check Prime
                    </button>
                </form>
                
                <div id=""loading"" class=""loading"">
                    <div class=""spinner""></div>
                </div>
                
                <div id=""resultContainer"" class=""result-container"">
                    <!-- Results will be displayed here -->
                </div>
            </div>
        </div>
    </div>

    <script>
        const form = document.getElementById('primeForm');
        const numberInput = document.getElementById('numberInput');
        const submitBtn = document.getElementById('submitBtn');
        const loading = document.getElementById('loading');
        const resultContainer = document.getElementById('resultContainer');
        const errorMessage = document.getElementById('errorMessage');

        form.addEventListener('submit', async (e) => {
            e.preventDefault();
            
            const number = parseInt(numberInput.value);
            
            if (!number || number < 1) {
                showError('Please enter a valid positive number');
                return;
            }
            
            if (number > Number.MAX_SAFE_INTEGER) {
                showError('Number is too large for accurate calculation');
                return;
            }
            
            hideError();
            showLoading(true);
            
            try {
                const response = await fetch('/api/check-prime', {
                    method: 'POST',
                    headers: {
                        'Content-Type': 'application/json',
                    },
                    body: JSON.stringify({ number: number })
                });
                
                const data = await response.json();
                
                if (response.ok) {
                    displayResult(data);
                } else {
                    showError(data.error || 'An error occurred');
                }
            } catch (error) {
                showError('Network error. Please try again.');
            } finally {
                showLoading(false);
            }
        });

        function showLoading(show) {
            loading.classList.toggle('show', show);
            submitBtn.disabled = show;
            if (show) {
                submitBtn.innerHTML = '<div class=""spinner"" style=""width: 16px; height: 16px; border-width: 2px;""></div> Calculating...';
            } else {
                submitBtn.innerHTML = '<i class=""material-icons"">search</i> Check Prime';
            }
        }

        function showError(message) {
            errorMessage.textContent = message;
            errorMessage.classList.add('show');
            numberInput.style.borderColor = 'var(--error-color)';
        }

        function hideError() {
            errorMessage.classList.remove('show');
            numberInput.style.borderColor = '';
        }

        function displayResult(data) {
            const isPrime = data.isPrime;
            const factors = data.factors || [];
            
            let resultHTML = `
                <div class=""result-card ${isPrime ? 'result-prime' : 'result-not-prime'}"">
                    <div class=""result-title"">
                        <i class=""material-icons"">${isPrime ? 'verified' : 'cancel'}</i>
                        ${data.number} is ${isPrime ? 'PRIME' : 'NOT PRIME'}
                    </div>
                    <div class=""result-explanation"">
                        ${data.explanation}
                    </div>
            `;
            
            if (!isPrime && factors.length > 0) {
                resultHTML += `
                    <div class=""factors-container"">
                        <div class=""factors-title"">Factors found:</div>
                        <div class=""factors-list"">
                            ${factors.map(factor => `<span class=""factor-chip"">${factor}</span>`).join('')}
                        </div>
                    </div>
                `;
            }
            
            resultHTML += '</div>';
            
            resultContainer.innerHTML = resultHTML;
            resultContainer.classList.add('show');
        }

        // Auto-focus input on page load
        window.addEventListener('load', () => {
            numberInput.focus();
        });

        // Clear results when input changes
        numberInput.addEventListener('input', () => {
            resultContainer.classList.remove('show');
            hideError();
        });
    </script>
</body>
</html>";
        }
    }

    public class PrimeRequest
    {
        public long? Number { get; set; }
    }

    public class PrimeResponse
    {
        public long Number { get; set; }
        public bool IsPrime { get; set; }
        public long[] Factors { get; set; }
        public string Explanation { get; set; }
        public DateTime ProcessingTime { get; set; }
    }
}