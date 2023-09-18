# .NET 6 API with Redis, Serilog, Elasticsearch, and TheMoviesDB Integration

This project is a .NET 6 API that leverages Redis for caching, Serilog for structured logging with Elasticsearch integration, and retrieves movies data from TheMoviesDB API.

## Prerequisites

Before running this project, make sure you have the following installed:

- [.NET 6 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)
- [Docker](https://www.docker.com/)

## Setting Up TheMovieDb Account
Visit https://developers.themoviedb.org/3/getting-started/introduction and
follow process to get your API key and add to appsettings under TheMovieDbConfig


## Setting Up Redis and Elasticsearch with Docker
### Redis
1. Pull the Redis Docker image: 
   - `docker pull redis`
2. Create a Redis container: 
   - `docker run -d --name redis-container -p 6379:6379 redis`

### Elasticsearch
1. Pull the Elasticsearch Docker image:
    - `docker pull docker.elastic.co/elasticsearch/elasticsearch:7.15.1`
2. Create an Elasticsearch container:
    - `docker run -d --name elasticsearch-container -p 9200:9200 -p 9300:9300 -e "discovery.type=single-node" docker.elastic.co/elasticsearch/elasticsearch:7.15.1`

## Running the Project
1. Clone this repository to your local machine. 
2. Open a terminal and navigate to the project directory. 
3. Build and run the project:
   - `dotnet run`
4. The API should now be running at http://localhost:5419/swagger/index.html with swagger 
showing all endpoints

## Logging and Elasticsearch
This project uses Serilog for structured logging. Logs are sent to Elasticsearch, which can be accessed at `http://localhost:9200` in your browser.

## Using Redis for Caching
This API utilizes Redis for caching movie data. The caching duration can be configured in the `appsettings.json` file.
