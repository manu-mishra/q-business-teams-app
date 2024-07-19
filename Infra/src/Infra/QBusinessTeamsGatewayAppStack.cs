using Amazon.CDK;
using Constructs;
using Infra.ApiGateway;
using Infra.Dynamo;
using Infra.Lambda;

namespace Infra
{
    public class QBusinessTeamsGatewayAppStack : Stack
    {
        internal QBusinessTeamsGatewayAppStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // DynamoDB Table
            var table = this.ConfigureChatSessionsTable();

            // Main Lambda Function
            var chatApiFunction = this.ConfigureChatApiLambda(table);

            // Authorizer Lambda Function
            var authorizerFunction = this.ConfigureAuthorizerLambda();

            this.ConfigureApiGateway(authorizerFunction, chatApiFunction);
        }
    }
}
