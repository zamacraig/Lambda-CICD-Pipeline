using Amazon.CDK;
using Amazon.CDK.AWS.Lambda;
using Amazon.CDK.AWS.S3;
using Constructs;
using Amazon.CDK.AWS.IAM;
using Role = Amazon.CDK.AWS.IAM.Role;
using Code = Amazon.CDK.AWS.Lambda.Code;
using Function = Amazon.CDK.AWS.Lambda.Function;
using Amazon.CDK.AWS.Events;
using Amazon.CDK.AWS.Events.Targets;


namespace LambdaCicdPipeline
{
    public class LambdaCicdPipelineStack : Stack
    {
        internal LambdaCicdPipelineStack(Construct scope, string id, IStackProps props = null) : base(scope, id, props)
        {
            // Define the S3 bucket and objects
            var bucket = Bucket.FromBucketName(this, "Bucket", "lambdalayerpackages001");

            // Define the IAM role for the Lambda function
            var lambdaRole = new Role(this, "LambdaExecutionRole", new RoleProps
            {
                RoleName = "Finance-Lambda-execute-Role",
                AssumedBy = new ServicePrincipal("lambda.amazonaws.com"),
                ManagedPolicies = new[]
                {
                    ManagedPolicy.FromAwsManagedPolicyName("service-role/AWSLambdaBasicExecutionRole")
                }
            });

            // Define the Lambda layer
            var lambdaLayer = new LayerVersion(this, "MyLambdaLayer", new LayerVersionProps
            {
                LayerVersionName = "Finance-Data-Layer",
                Code = Code.FromBucket(bucket, "FinanceData/layer.zip"),
                CompatibleRuntimes = new[] { Runtime.PYTHON_3_9 }
            });

            // Define the Python Lambda function
            var lambdaFunction = new Function(this, "MyPythonLambdaFunction", new FunctionProps
            {
                FunctionName = "Finance-Data-Func",
                Runtime = Runtime.PYTHON_3_9,
                Handler = "lambda_function.lambda_handler",
                Code = Code.FromAsset("src/LambdaFunction/Code"),
                Role = lambdaRole,
                Layers = new[] { lambdaLayer }
            });

            // Create an EventBridge rule
            var rule = new Rule(this, "LambdaExecuteRule", new RuleProps
            {
                RuleName = "Finance-Lambda-Execute-Rule",
                Schedule = Schedule.Cron(new CronOptions
                {
                    Minute = "0",
                    Hour = "7"
                })
            });

            // Add the Lambda function as the target of the rule
            rule.AddTarget(new LambdaFunction(lambdaFunction));
        }
    }
}
