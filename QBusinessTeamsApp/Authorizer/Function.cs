using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.RuntimeSupport;
using Amazon.Lambda.Serialization.SystemTextJson;
using System.Text.Json.Serialization;

namespace Authorizer;

public class Function
{
    private static async Task Main()
    {
        Func<APIGatewayCustomAuthorizerRequest, ILambdaContext, APIGatewayCustomAuthorizerResponse> handler = FunctionHandler;
        await LambdaBootstrapBuilder.Create(handler, new SourceGeneratorLambdaJsonSerializer<LambdaFunctionJsonSerializerContext>())
            .Build()
            .RunAsync();
    }

    public static APIGatewayCustomAuthorizerResponse FunctionHandler(APIGatewayCustomAuthorizerRequest input, ILambdaContext context)
    {
        context.Logger.LogLine($"Received token: {input.AuthorizationToken}");
        return GeneratePolicy("user", "Allow", input.MethodArn);
        // Check if the token starts with 'Bearer '
        if (!string.IsNullOrEmpty(input.AuthorizationToken) && input.AuthorizationToken.StartsWith("Bearer ", StringComparison.Ordinal))
        {
            return GeneratePolicy("user", "Allow", input.MethodArn);
        }
        else
        {
            return GeneratePolicy("user", "Deny", input.MethodArn);
        }
    }

    private static APIGatewayCustomAuthorizerResponse GeneratePolicy(string principalId, string effect, string resource)
    {
        var authResponse = new APIGatewayCustomAuthorizerResponse
        {
            PrincipalID = principalId
        };

        var policyDocument = new APIGatewayCustomAuthorizerPolicy
        {
            Version = "2012-10-17",
            Statement = new List<APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement>
            {
                new APIGatewayCustomAuthorizerPolicy.IAMPolicyStatement
                {
                    Action = new HashSet<string> { "execute-api:Invoke" },
                    Effect = effect,
                    Resource = new HashSet<string> { resource }
                }
            }
        };

        authResponse.PolicyDocument = policyDocument;
        return authResponse;
    }
}

[JsonSerializable(typeof(APIGatewayCustomAuthorizerRequest))]
[JsonSerializable(typeof(APIGatewayCustomAuthorizerResponse))]
public partial class LambdaFunctionJsonSerializerContext : JsonSerializerContext
{
}
