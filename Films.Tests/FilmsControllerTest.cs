using AutoMapper;
using Films.Controllers;
using Films.Core;
using Films.Data;
using Films.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Xunit;

namespace Films.Tests
{
    public class FilmsControllerTest
    {
        [Fact]
        public async void Post_ReturnsBadRequestObjectWithMessageToAddFilmFirst_ForNewFilmWhichContainsCast()
        {
            //Arrange
            var repository = new FakeFilmRepository();
            repository.Add(new Director() { Id = 1, FirstName = "Quentin", LastName = "Tarantino" });
            repository.Add(new Actor() { Id = 1, FirstName = "Kurt", LastName = "Russell", ActorFilms = new List<ActorFilm>() { new ActorFilm() { FilmId = 1, ActorId = 1 } } });
            repository.Add(new Film() { Id = 1, DirectorId = 1, Title = "The Hateful Eight", Cast = new List<ActorFilm>() { new ActorFilm() { FilmId = 1, ActorId = 1 } } });

            await repository.SaveChangesAsync();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<FilmProfile>();
            });

            var mapper = config.CreateMapper();

            var controller = new FilmsController(repository, mapper);

            var newFilm = new FilmModel() { Cast = new List<ActorModel>() { new ActorModel() { FirstName = "Kurt", LastName = "Russell"} }, Title = "The Hateful Eight" };

            //Act
            var result = await controller.Post(newFilm);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Please add film first. Update films cast with updatefilm method", (result.Result as BadRequestObjectResult).Value);

        }

        [Fact]
        public async void Post_ReturnsBadRequestObjectWithMessageAboutRequiringADirector_ForNewFilmWhichContainsCast()
        {
            //Arrange
            var repository = new FakeFilmRepository();
            repository.Add(new Director() { Id = 1, FirstName = "Quentin", LastName = "Tarantino" });
            repository.Add(new Actor() { Id = 1, FirstName = "Kurt", LastName = "Russell", ActorFilms = new List<ActorFilm>() { new ActorFilm() { FilmId = 1, ActorId = 1 } } });
            repository.Add(new Film() { Id = 1, DirectorId = 1, Title = "The Hateful Eight", Cast = new List<ActorFilm>() { new ActorFilm() { FilmId = 1, ActorId = 1 } } });

            await repository.SaveChangesAsync();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<FilmProfile>();
            });

            var mapper = config.CreateMapper();

            var controller = new FilmsController(repository, mapper);

            var newFilm = new FilmModel() { Title = "The Hateful Eight" };

            //Act
            var result = await controller.Post(newFilm);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Director required", (result.Result as BadRequestObjectResult).Value);

        }

        [Fact]
        public async void Post_ReturnsBadRequestObjectWithMessageThatFilmAlreadyExists_ForNewFilmWhichContainsCast()
        {
            //Arrange
            var repository = new FakeFilmRepository();
            repository.Add(new Director() { Id = 1, FirstName = "Quentin", LastName = "Tarantino" });
            repository.Add(new Actor() { Id = 1, FirstName = "Kurt", LastName = "Russell", ActorFilms = new List<ActorFilm>() { new ActorFilm() { FilmId = 1, ActorId = 1 } } });
            repository.Add(new Film() { Id = 1, DirectorId = 1, Title = "The Hateful Eight", Cast = new List<ActorFilm>() { new ActorFilm() { FilmId = 1, ActorId = 1 } } });

            await repository.SaveChangesAsync();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<FilmProfile>();
            });

            var mapper = config.CreateMapper();

            var controller = new FilmsController(repository, mapper);

            var newFilm = new FilmModel() { Title = "The Hateful Eight", Director = new DirectorModel() { FirstName = "Quentin", LastName = "Tarantino" } };

            //Act
            var result = await controller.Post(newFilm);

            //Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("There is a film with same name", (result.Result as BadRequestObjectResult).Value);

        }
    }
}
