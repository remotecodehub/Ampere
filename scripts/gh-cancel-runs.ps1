gh run list --status queued --json databaseId --jq '.[].databaseId' | ForEach-Object { gh run cancel  }
