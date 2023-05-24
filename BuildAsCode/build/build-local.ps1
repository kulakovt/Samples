# Run it from solution root folder
./build.ps1 `
  -Counter 42 `
  -VcsNumber 'a42b' `
  -Branch 'test-local' `
  -PullRequestNumber '24' `
  -PullRequestSourceBranch 'local' `
  -DockerRepositoriesUrl 'https://localhost/harbor/projects/1/repositories' `
  -DockerProjectName 'library' `
  -NuGetUrl 'https://localhost' `
  -NuGetFeedName 'Nuget-Push' `
  -NuGetKey 'null' `
  -OctopusProjectsUrl 'https://localhost/app#/Spaces-1/projects' `
  -OctopusKey 'null' `
  -DotNetToolUrl 'https://localhost'
