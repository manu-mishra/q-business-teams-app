using Amazon.CDK;
using Amazon.CDK.AWS.DynamoDB;
using Constructs;
using Attribute = Amazon.CDK.AWS.DynamoDB.Attribute;

namespace Infra.Dynamo
{
    internal static class DynamoDbConstructs
    {
        public static Table ConfigureChatSessionsTable(this Construct scope)
        {
            var table = new Table(scope, "ChatSessions", new TableProps
            {
                TableName = "ChatSessions",
                PartitionKey = new Attribute
                {
                    Name = "id",
                    Type = AttributeType.STRING
                },
                RemovalPolicy = RemovalPolicy.DESTROY
            });
            return table;
        }
    }
}
