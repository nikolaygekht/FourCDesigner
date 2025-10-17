# Deployment Guide

The project must be deployed as a docker container. 

Immutable content (e.g. pages, scripts) must be stored in the container. 

Mutable content (e.g. configuration, database, logs) must be stored in the volume. 

Use `mcr.microsoft.com/dotnet/aspnet:8.0` as the base container. 

The following bash scripts must be prepared in the addition to the `dockerfile`

 * `build-docker.sh` that builds the docker image
 * `build-all.sh` that 
   * builds the whole project
   * runs the tests 
   * and builds the docker image.
 * `build-all.sh` script must fail if building of the project or running the test fails. 
 * `create-volume.sh` that creates the volume.
 * `copy-to-volume.sh` that copies sample configuration to the volume. 
 * `run.sh` that starts the docker
 * `stop.sh` that stops and deletes the docker container (but not the volume!)