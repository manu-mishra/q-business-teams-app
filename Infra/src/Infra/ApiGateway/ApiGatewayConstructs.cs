using Amazon.CDK;
using Amazon.CDK.AWS.APIGateway;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Constructs;
using System.Collections.Generic;

namespace Infra.ApiGateway
{
    internal static class ApiGatewayConstructs
    {
        public static LambdaRestApi ConfigureApiGateway(this Construct scope, Function authorizerFunction, Function apiFunction)
        {
            var apiGatewayName = "TeamsGatewayApi";
            var apiGatewayLoggingRole = new Role(scope, "ApiGatewayLoggingRole", new RoleProps
            {
                AssumedBy = new ServicePrincipal("apigateway.amazonaws.com"),
                ManagedPolicies = new[]
                {
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AmazonAPIGatewayPushToCloudWatchLogs")
                }
            });

            // Create the API Gateway account resource with the logging role
            var apiGatewayAccount = new Amazon.CDK.CfnResource(scope, "ApiGatewayAccount", new Amazon.CDK.CfnResourceProps
            {
                Type = "AWS::ApiGateway::Account",
                Properties = new Dictionary<string, object>
                {
                   { "CloudWatchRoleArn", apiGatewayLoggingRole.RoleArn }
                }
            });
            // Request Authorizer
            var lambdaAuthorizer = new RequestAuthorizer(scope, "LambdaAuthorizer", new RequestAuthorizerProps
            {
                Handler = authorizerFunction,
                IdentitySources = new[] { IdentitySource.Header("Authorization") },
                ResultsCacheTtl = Duration.Minutes(5)
            });

            var logGroup = new LogGroup(scope, "ApiGatewayLogGroup", new LogGroupProps
            {
                LogGroupName = $"/aws/apigateway/{apiGatewayName}",
                Retention = RetentionDays.ONE_DAY,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            // API Gateway as a Proxy to the Lambda Function with Authorizer
            var api = new LambdaRestApi(scope, apiGatewayName, new LambdaRestApiProps
            {
                Handler = apiFunction,
                RestApiName = apiGatewayName,
                Proxy = true,
                DefaultMethodOptions = new MethodOptions
                {
                    AuthorizationType = AuthorizationType.CUSTOM,
                    Authorizer = lambdaAuthorizer
                },
                DeployOptions = new StageOptions
                {
                    MetricsEnabled = true,
                    TracingEnabled = true,
                    DataTraceEnabled = false,
                    AccessLogDestination = new LogGroupLogDestination(logGroup),
                    AccessLogFormat = AccessLogFormat.JsonWithStandardFields(),
                    LoggingLevel = MethodLoggingLevel.INFO
                }
            });
            api.DeploymentStage.Node.AddDependency(apiGatewayAccount);
            return api;
        }
    }
}
