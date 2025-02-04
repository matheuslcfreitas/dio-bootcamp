using System.Text.RegularExpressions;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Threading.Tasks;
using System.Linq;

namespace FuncMatheusCpf
{
    public static class CpfValidatorFunction
    {
        [Function("funcmatheuscpf")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "validate-cpf/{cpf}")] HttpRequestData req,
            string cpf,
            FunctionContext context)
        {
            var logger = context.GetLogger("CpfValidatorFunction");
            logger.LogInformation($"Validating CPF: {cpf}");

            var response = req.CreateResponse();

            if (string.IsNullOrWhiteSpace(cpf))
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync("CPF is required.");
                return response;
            }

            if (IsValidCpf(cpf))
            {
                response.StatusCode = HttpStatusCode.OK;
                await response.WriteStringAsync("Valid CPF.");
            }
            else
            {
                response.StatusCode = HttpStatusCode.BadRequest;
                await response.WriteStringAsync("Invalid CPF.");
            }

            return response;
        }

        private static bool IsValidCpf(string cpf)
        {
            // Remove non-numeric characters
            cpf = Regex.Replace(cpf, "[^0-9]", "");

            if (cpf.Length != 11 || cpf.All(c => c == cpf[0]))
                return false;

            int[] multiplicadores1 = { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicadores2 = { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCpf = cpf.Substring(0, 9);
            int soma = multiplicadores1.Select((t, i) => int.Parse(tempCpf[i].ToString()) * t).Sum();
            int resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            string digito = resto.ToString();

            tempCpf += digito;
            soma = multiplicadores2.Select((t, i) => int.Parse(tempCpf[i].ToString()) * t).Sum();
            resto = soma % 11;
            resto = resto < 2 ? 0 : 11 - resto;
            digito += resto.ToString();

            return cpf.EndsWith(digito);
        }
    }
}
