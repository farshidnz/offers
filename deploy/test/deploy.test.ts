import { App } from "aws-cdk-lib";
import { Template } from "aws-cdk-lib/assertions";
import * as dotenv from "dotenv";
import { DotnetStack } from "../lib";
dotenv.config();
describe("DotnetStack", () => {
  test("synthesizes the way we expect", () => {
    const app = new App();

    const testStack = new DotnetStack(app, "MyTestStack", {
      env: {
        account: process.env.AWS_ACCOUNT_ID,
        region: process.env.AWS_REGION,
      },
    });

    const template = Template.fromStack(testStack);

    template.hasResource("AWS::ECS::Service", {});
  });
});
