#!/usr/bin/env node
import { getEnv, getResourceName } from "@cashrewards/cdk-lib";
import { App, StackProps } from "aws-cdk-lib";
import * as dotenv from "dotenv";
import { existsSync } from "fs";
import * as path from "path";
import "source-map-support/register";
import { DotnetStack } from "./lib";

export class OffersApp extends App {
  protected stackProps: StackProps;
  constructor() {
    super();
    this.stackProps = {
      env: {
        account: getEnv("AWS_ACCOUNT_ID"),
        region: getEnv("AWS_REGION"),
      },
    };
  }

  start() {
    new DotnetStack(
      this,
      getResourceName(getEnv("PROJECT_NAME")),
      this.stackProps
    );
  }
}

(async () => {
  const envFile = path.resolve(process.cwd(), ".env");
  if (existsSync(envFile)) {
    dotenv.config({
      path: envFile,
    });
  } else {
    dotenv.config();
  }

  const app = new OffersApp();
  app.start();
})();
