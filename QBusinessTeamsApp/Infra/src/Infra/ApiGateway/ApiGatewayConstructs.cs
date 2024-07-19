using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.Lambda;
using Constructs;

namespace Infra.ApiGateway
{
    internal static class ApiGatewayConstructs
    {
        public static LambdaRestApi ConfigureApiGateway(this Construct scope, Function authorizerFunction, Function apiFunction)
        {
            // Request Authorizer
            //var lambdaAuthorizer = new RequestAuthorizer(scope, "LambdaAuthorizer", new RequestAuthorizerProps
            //{
            //    Handler = authorizerFunction,
            //    IdentitySources = new[] { IdentitySource.Header("Authorization") },
            //    ResultsCacheTtl = Duration.Minutes(5)
            //});

            // API Gateway as a Proxy to the Lambda Function with Authorizer
            var api = new LambdaRestApi(scope, "TeamsGatewayApp", new LambdaRestApiProps
            {
                Handler = apiFunction,
                RestApiName = "TeamsGatewayApp",
                Proxy = true,
                //DefaultMethodOptions = new MethodOptions
                //{
                //    AuthorizationType = AuthorizationType.CUSTOM,
                //    Authorizer = lambdaAuthorizer
                //}
            });

            return api;
        }
    }
}
