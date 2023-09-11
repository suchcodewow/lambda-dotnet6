using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

using Amazon.Lambda.Core;
using Amazon.Lambda.APIGatewayEvents;

// Dynatrace-Start
using Dynatrace.OpenTelemetry;
using Dynatrace.OpenTelemetry.Instrumentation.AwsLambda;
using OpenTelemetry;
using OpenTelemetry.Instrumentation.AWSLambda;
using OpenTelemetry.Trace;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace HelloWorld
{

    public class Function
    {

        private static readonly TracerProvider TracerProvider;

        static Function()
        {
            DynatraceSetup.InitializeLogging();
            TracerProvider = Sdk.CreateTracerProviderBuilder()
                .AddDynatrace()
                // Configures AWS Lambda invocations tracing
                .AddAWSLambdaConfigurations(c => c.DisableAwsXRayContextExtraction = true)
                // Instrumentation for creation of span (Activity) representing AWS SDK call.
                // Can be omitted if there are no outgoing AWS SDK calls to other AWS Lambdas and/or calls to AWS services like DynamoDB and SQS.
                .AddAWSInstrumentation(c => c.SuppressDownstreamInstrumentation = true)
                // Adds injection of Dynatrace-specific context information in certain SDK calls (e.g. Lambda Invoke).
                // Can be omitted if there are no outgoing calls to other Lambdas via the AWS Lambda SDK.
                .AddDynatraceAwsSdkInjection()
                .Build();
        }

        public APIGatewayHttpApiV2ProxyResponse FunctionHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            return AWSLambdaWrapper.Trace(TracerProvider, FunctionHandlerInternal, request, context);
        }

        private APIGatewayHttpApiV2ProxyResponse FunctionHandlerInternal(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            // This is just an example of function handler and should be replaced by actual code.

            return new APIGatewayHttpApiV2ProxyResponse
            {
                StatusCode = 200,
                Body = "Example function result",
            };
        }
    }
}
