#!/bin/bash

dotnet test --logger:"junit;LogFilePath=/app/inttestout/{assembly}.xml" app/CashrewardsOffers/tests/Application.AcceptanceTests/Application.AcceptanceTests.csproj
dotnet test --logger:"junit;LogFilePath=/app/inttestout/{assembly}.xml" app/CashrewardsOffers/tests/Application.IntegrationTests/Application.IntegrationTests.csproj

chown -R $1 /app