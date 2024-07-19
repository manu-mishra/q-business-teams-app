using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.Logs;
using Constructs;
using System.Collections.Generic;
using LogGroupProps = Amazon.CDK.AWS.Logs.LogGroupProps;

namespace Infra.Lambda
{
    internal static class LambdaConstructs
    {
        public static Function ConfigureChatApiLambda(this Construct scope, Table chatSessionDynamoDbTable)
        {
            string functionName = "ChatApiFunction";
            var logGroup = new LogGroup(scope, "ChatApiLogGroup", new LogGroupProps
            {
                LogGroupName = $"/aws/lambda/{functionName}",
                Retention = RetentionDays.ONE_DAY,
                RemovalPolicy = RemovalPolicy.DESTROY
            });

            var lambdaFunction =
            new Function(scope, "ChatApiFunction", new FunctionProps
            {
                FunctionName = functionName,
                Runtime = Runtime.DOTNET_8,
                Code = Code.FromAsset("../Api/bin/release/net8.0"),
                Handler = "Api",
                Environment = new Dictionary<string, string>
                {
                    { "TABLE_NAME", chatSessionDynamoDbTable.TableName }
                },
                LogGroup = logGroup,
                Tracing = Tracing.ACTIVE,
                Architecture = Architecture.ARM_64
            });
            chatSessionDynamoDbTable.GrantReadWriteData(lambdaFunction);
            return lambdaFunction;
        }

        public static Function ConfigureAuthorizerLambda(this Construct scope)
        {
            string functionName = "ApiGatewayAuthorizerFunction";
            var logGroup = new LogGroup(scope, "ApiGatewayAuthorizerFunctionLogGroup", new LogGroupProps
            {
                LogGroupName = $"/aws/lambda/{functionName}",
                Retention = RetentionDays.ONE_DAY,
                RemovalPolicy = RemovalPolicy.DESTROY
            });
            var lambdaFunction =
            new Function(scope, "ApiGatewayAuthorizerFunction", new FunctionProps
            {
                FunctionName = functionName,
                Runtime = Runtime.DOTNET_8,
                Code = Code.FromAsset("../Authorizer/bin/release/net8.0"),
                Handler = "Authorizer::Authorizer.Function::Main",
                LogGroup = logGroup,
                Tracing = Tracing.ACTIVE,
                Architecture = Architecture.ARM_64,
            });
            return lambdaFunction;
        }
    }
}

