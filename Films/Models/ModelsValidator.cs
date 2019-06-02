using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Films.Models
{
    public static class ModelsValidator
    {
        public static Tuple<bool, string> ValidateFilmModels(FilmIndustryEmployee model)
        {
            if (model.Films != null && model.Films.Count() > 0)
            {
                for (int i = 0; i < model.Films.Count(); i++)
                {
                    var film = model.Films.ElementAt(i);

                    if (film.Cast != null && film.Cast.Count() > 0) return Tuple.Create(false, $"{film.Title} contains Cast inforamtion. Please use film controller in order to update cast");
                    if (film.Director != null) return Tuple.Create(false, "Film should not contain director in order to execute this method.");

                }
            }

            return Tuple.Create(true, "");
        }

        public static Tuple<bool, string> ValidateDirectorModel(FilmModel film)
        {
            if (film.Director == null) return Tuple.Create(false, "Director required");

            if (string.IsNullOrEmpty(film.Director.FirstName) || string.IsNullOrEmpty(film.Director.LastName)) return Tuple.Create(false, "Directors name invalid");

            if (film.Director.Films != null && film.Director.Films.Count() > 0) return Tuple.Create(false, $"Director's entity {film.Director.FirstName} {film.Director.LastName} contains films. " +
                "Adding films for a particular director is not supported by films controller. " +
                "If you want to update/add films to director's entity please use directors controller");

            return Tuple.Create(true, "");
        }

        public static Tuple<bool, string> ValidateActorModels(FilmModel film)
        {
            if (film.Cast != null && film.Cast.Count() > 0)
            {
                var actors = film.Cast;
                foreach (var actor in actors)
                {
                    if (string.IsNullOrEmpty(actor.FirstName) || string.IsNullOrEmpty(actor.LastName)) return Tuple.Create(false, $"{actor.FirstName} {actor.LastName} invalid");

                    if (actor.Films != null && actor.Films.Count() > 0) return Tuple.Create(false, $"{actor.FirstName} {actor.LastName} contains films. " +
                        $"Adding films for a particular actor is not supported by films controller. " +
                            $"If you want to add films to actor's entity please use actors controller or AddActorToCast method");
                }
            }
            return Tuple.Create(true, "");
        }
    }
}
