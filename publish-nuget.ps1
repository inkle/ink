#!/bin/pwsh

# do not bother with pull requests
if ($env:TRAVIS_PULL_REQUEST -ne "false") {
    Write-Host "Skipping NuGet publish on pull request"
    exit 0 
}

# set release branch to master by default
$releaseBranch = $env:RELEASE_BRANCH
if ("$releaseBranch" -eq "") {
    $releaseBranch = "master"
}

# check if we're building a tagged release build
$tag = "$($env:TRAVIS_TAG)"
if (("$($env:TRAVIS_BRANCH)" -ne $tag) -and ("$($env:TRAVIS_BRANCH)" -ne $releaseBranch)) {
    Write-Host "Skipping NuGet publish for the branch $($env:TRAVIS_BRANCH)"
    exit 0
}

# extract version number in case it has prefix (like v1.2.3)
if (($tag -ne "") -and ($tag -match '\d+(\.\d+)+$')) {
    $tag = $Matches[0]
}

# delete existing nuget packages, although this should never happen on travis
if ((Test-Path ink-engine-runtime/bin/Release) -and (@(Get-ChildItem ink-engine-runtime/bin/Release/*.nupkg).Count -gt 0)) {
    Remove-Item ink-engine-runtime/bin/Release/*.nupkg
}

if ($tag -eq "") {
    # if it's not a tagged build, then it's either a nightly or a master push
    $feedUrl = "$($env:NIGHTLY_FEED_SOURCE_URL)"
    $apiKey = "$($env:NIGHTLY_FEED_APIKEY)"
    if (($feedUrl -eq "") -or ($apiKey -eq "")) {
        Write-Host "Nightly feed is not set up, skipping pre-release build"
        exit 0
    }

    # check if it's a scheduled nightly build
    if ($env:TRAVIS_EVENT_TYPE -eq "cron") {
        # check if there were any commits in the last 24 hours
        $commits = "$(git log -1 --since=1.day --pretty=format:"%h %s")"
        $suffix = ""
        if ($commits -ne "") {
            Write-Host "Found commit: $commits"
            $timestamp = [DateTime]::UtcNow.ToString("yyMMddHH")
            $suffix = "nightly-$timestamp"
        }
    }
    elseif ("$($env:PUBLISH_MASTER_BUILDS)" -eq "true") {
        $timestamp = [DateTime]::UtcNow.ToString("yyMMddHHmmss")
        $suffix = "master-$timestamp"
    }
    if ("$suffix" -eq "") {
        Write-Host "Skipping publishing the pre-release build"
        exit 0
    }

    # get the latest tag in the branch history
    $tag = "$(git describe --tags $(git rev-list --tags --max-count=1))"
    # extract version number in case it has prefix (like v1.2.3)
    if (($tag -ne "") -and ($tag -match '\d+(\.\d+)+$')) {
        $tag = $Matches[0]
    }
    if ($tag -ne "") {
        Write-Host "Building a pre-release build $tag-$suffix..."
        dotnet pack -c Release /p:VersionPrefix=$tag --version-suffix "$suffix" ink-engine-runtime/ink-engine-runtime.csproj
    } else {
        Write-Host "Building a pre-release build $suffix..."
        dotnet pack -c Release --version-suffix "$suffix" ink-engine-runtime/ink-engine-runtime.csproj
    }
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    Write-Host "Publishing pre-release build..."
    $packageName = @(Get-ChildItem ink-engine-runtime/bin/Release/*.nupkg)[0]
}
else {
    # tagged build will only happen on a tag push, so this means we're building a release package
    $feedUrl = "$($env:RELEASE_FEED_SOURCE_URL)"
    $apiKey = "$($env:RELEASE_FEED_APIKEY)"
    if (($feedUrl -eq "") -or ($apiKey -eq "")) {
        Write-Host "Release feed is not set up, skipping release build"
        exit 0
    }

    Write-Host "Building a release build..."
    dotnet pack -c Release /p:VersionPrefix=$tag ink-engine-runtime/ink-engine-runtime.csproj
    if ($LASTEXITCODE -ne 0) {
        exit $LASTEXITCODE
    }

    Write-Host "Publishing release build..."
    $packageName = @(Get-ChildItem ink-engine-runtime/bin/Release/*.nupkg)[0]
}
Write-Host "Pushing package $($packageName.FullName)..."
dotnet nuget push $packageName.FullName --api-key $apiKey --source $feedUrl
exit $LASTEXITCODE
