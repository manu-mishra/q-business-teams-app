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
            var lambdaFunction =
            new Function(scope, "ChatApiFunction", new FunctionProps
            {
                FunctionName = "ChatApiFunction",
                Runtime = Runtime.DOTNET_6,
                Code = Code.FromAsset("../Api/bin/release/net8.0"),
                Handler = "Api",
                Environment = new Dictionary<string, string>
                {
                    { "TABLE_NAME", chatSessionDynamoDbTable.TableName }
                }
            });

            new LogGroup(scope, "ChatApiLogGroup", new LogGroupProps
            {
                LogGroupName = $"/aws/lambda/{lambdaFunction.FunctionName}",
                Retention = RetentionDays.ONE_DAY,
                RemovalPolicy = RemovalPolicy.DESTROY // Automatically delete the log group when the stack is deleted
            });

            chatSessionDynamoDbTable.GrantReadWriteData(lambdaFunction);
            return lambdaFunction;
        }

        public static Function ConfigureAuthorizerLambda(this Construct scope)
        {
            var lambdaFunction =
            new Function(scope, "ApiGatewayAuthorizerFunction", new FunctionProps
            {
                FunctionName = "ApiGatewayAuthorizerFunction",
                Runtime = Runtime.DOTNET_6,
                Code = Code.FromAsset("../Authorizer/bin/release/net8.0"),
                Handler = "Authorizer::Authorizer.Function::Main"
            });

            new LogGroup(scope, "AuthorizerLogGroup", new LogGroupProps
            {
                LogGroupName = $"/aws/lambda/{lambdaFunction.FunctionName}",
                Retention = RetentionDays.ONE_DAY,
                RemovalPolicy = RemovalPolicy.DESTROY
            });
            return lambdaFunction;
        }
    }
}

