# .NET 6 MoviesHub API with Redis and Hubtel SMS

```bash
# install dependencies
$ dotnet restore

# setup Hubtel SMS account
$ visit https://developers.hubtel.com/ to sign up
$ follow procedures to create keys and add to appsettings under HubtelSmsConfig

# setup TheMovieDb account
$ visit https://developers.themoviedb.org/3/getting-started/introduction 
$ follow process to get your API key and add to appsettings under TheMovieDbConfig

# install redis using docker with the following commands
$ docker pull redis
$ docker run --name redis -d redis

# run project
$ start redis in docker and run app
$ run app

# client-app
https://github.com/devpaulmensah/vue_movies_hub
```