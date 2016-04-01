#region Copyright (C) 2007-2015 Team MediaPortal

/*
    Copyright (C) 2007-2015 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using System.Collections.Generic;
using System.IO;
using System.Web;
using MediaPortal.Extensions.OnlineLibraries.Libraries.Common;
using Newtonsoft.Json;
using MediaPortal.Common.Logging;
using MediaPortal.Common;
using MediaPortal.Extensions.OnlineLibraries.Libraries.OmDbV1.Data;

namespace MediaPortal.Extensions.OnlineLibraries.Libraries.OmDbV1
{
  internal class OmDbApiV1
  {
    //Images cannot currently be downloaded

    #region Constants

    public const string DefaultLanguage = "en";

    private const string URL_API_BASE = "http://www.omdbapi.com/";
    private const string URL_QUERYMOVIE = URL_API_BASE + "?s={0}&type=movie";
    private const string URL_QUERYSERIES = URL_API_BASE + "?s={0}&type=series";
    private const string URL_GETTITLEMOVIE = URL_API_BASE + "?t={0}&type=movie";
    private const string URL_GETIMDBIDMOVIE =   URL_API_BASE + "?i={0}&type=movie";
    private const string URL_GETTITLESERIES = URL_API_BASE + "?t={0}&type=series";
    private const string URL_GETIMDBIDSERIES = URL_API_BASE + "?i={0}&type=series";
    private const string URL_GETTITLESEASON = URL_API_BASE + "?t={0}&Season={1}&type=series";
    private const string URL_GETIMDBIDSEASON =  URL_API_BASE + "?i={0}&Season={1}&type=series";
    private const string URL_GETTITLEEPISODE = URL_API_BASE + "?t={0}&Season={1}&episode={2}&type=episode";
    private const string URL_GETIMDBIDEPISODE =  URL_API_BASE + "?i={0}&Season={1}&episode={2}&type=episode";

    #endregion

    #region Fields

    private readonly string _cachePath;
    private readonly Downloader _downloader;

    #endregion

    #region Constructor

    public OmDbApiV1(string cachePath)
    {
      _cachePath = cachePath;
      _downloader = new Downloader { EnableCompression = true };
      _downloader.Headers["Accept"] = "application/json";
    }

    #endregion

    #region Public members

    /// <summary>
    /// Search for movies by name given in <paramref name="title"/>.
    /// </summary>
    /// <param name="title">Full or partly name of movie</param>
    /// <returns>List of possible matches</returns>
    public List<SearchItem> SearchMovie(string title, int year)
    {
      string url = GetUrl(URL_QUERYMOVIE, year, false, false, HttpUtility.UrlEncode(title));
      SearchResult results = _downloader.Download<SearchResult>(url);
      foreach (SearchItem item in results.SearchResults) item.AssignProperties();
      if (results.ResponseValid == false) return null;
      return results.SearchResults;
    }

    /// <summary>
    /// Search for series by name given in <paramref name="title"/>.
    /// </summary>
    /// <param name="title">Full or partly name of series</param>
    /// <returns>List of possible matches</returns>
    public List<SearchItem> SearchSeries(string title, int year)
    {
      string url = GetUrl(URL_QUERYSERIES, year, false, false, HttpUtility.UrlEncode(title));
      SearchResult results = _downloader.Download<SearchResult>(url);
      if (results.ResponseValid == false) return null;
      foreach (SearchItem item in results.SearchResults) item.AssignProperties();
      return results.SearchResults;
    }

    /// <summary>
    /// Returns detailed information for a single <see cref="Movie"/> with given <paramref name="imdbId"/>. This method caches request
    /// to same movies using the cache path given in <see cref="OmDbApiV1"/> constructor.
    /// </summary>
    /// <param name="id">IMDB id of movie</param>
    /// <returns>Movie information</returns>
    public Movie GetMovie(string id)
    {
      string cache = CreateAndGetCacheName(id, "Movie");
      Movie returnValue = null;
      if (!string.IsNullOrEmpty(cache) && File.Exists(cache))
      {
        string json = File.ReadAllText(cache);
        returnValue = JsonConvert.DeserializeObject<Movie>(json);
      }
      else
      {
        string url = GetUrl(URL_GETIMDBIDMOVIE, 0, true, true, id);
        returnValue = _downloader.Download<Movie>(url, cache);
      }
      if (returnValue.ResponseValid == false) return null;
      if (returnValue != null) returnValue.AssignProperties();
      return returnValue;
    }

    /// <summary>
    /// Returns detailed information for a single <see cref="Series"/> with given <paramref name="id"/>. This method caches request
    /// to same series using the cache path given in <see cref="OmDbApiV1"/> constructor.
    /// </summary>
    /// <param name="id">IMDB id of Series</param>
    /// <returns>Series information</returns>
    public Series GetSeries(string id)
    {
      string cache = CreateAndGetCacheName(id, "Series");
      Series returnValue = null;
      if (!string.IsNullOrEmpty(cache) && File.Exists(cache))
      {
        string json = File.ReadAllText(cache);
        returnValue = JsonConvert.DeserializeObject<Series>(json);
      }
      else
      {
        string url = GetUrl(URL_GETIMDBIDSERIES, 0, true, true, id);
        returnValue = _downloader.Download<Series>(url, cache);
      }
      if (returnValue.ResponseValid == false) return null;
      if (returnValue != null) returnValue.AssignProperties();
      return returnValue;
    }

    /// <summary>
    /// Returns detailed information for a single <see cref="Season"/> with given <paramref name="id"/>. This method caches request
    /// to same seasons using the cache path given in <see cref="OmDbApiV1"/> constructor.
    /// </summary>
    /// <param name="id">IMDB id of series</param>
    /// <param name="season">Season number</param>
    /// <returns>Season information</returns>
    public Season GetSeriesSeason(string id, int season)
    {
      string cache = CreateAndGetCacheName(id, string.Format("Season{0}", season));
      Season returnValue = null;
      if (!string.IsNullOrEmpty(cache) && File.Exists(cache))
      {
        string json = File.ReadAllText(cache);
        returnValue = JsonConvert.DeserializeObject<Season>(json);
      }
      else
      {
        string url = GetUrl(URL_GETIMDBIDSEASON, 0, true, true, id, season);
        returnValue = _downloader.Download<Season>(url, cache);
      }
      if (returnValue.ResponseValid == false) return null;
      if (returnValue != null) returnValue.InitEpisodes();
      return returnValue;
    }

    /// <summary>
    /// Returns detailed information for a single <see cref="Episode"/> with given <paramref name="id"/>. This method caches request
    /// to same episodes using the cache path given in <see cref="OmDbApiV1"/> constructor.
    /// </summary>
    /// <param name="id">IMDB id of series</param>
    /// <param name="season">Season number</param>
    /// <param name="episode">Episode number</param>
    /// <returns>Episode information</returns>
    public Episode GetSeriesEpisode(string id, int season, int episode)
    {
      string cache = CreateAndGetCacheName(id, string.Format("Season{0}_Episode{1}", season, episode));
      Episode returnValue = null;
      if (!string.IsNullOrEmpty(cache) && File.Exists(cache))
      {
        string json = File.ReadAllText(cache);
        returnValue = JsonConvert.DeserializeObject<Episode>(json);
      }
      else
      {
        string url = GetUrl(URL_GETIMDBIDEPISODE, 0, true, true, id, season, episode);
        returnValue = _downloader.Download<Episode>(url, cache);
      }
      if (returnValue.ResponseValid == false) return null;
      if (returnValue != null) returnValue.AssignProperties();
      return returnValue;
    }

    #endregion

    #region Protected members

    /// <summary>
    /// Builds and returns the full request url.
    /// </summary>
    /// <param name="urlBase">Query base</param>
    /// <param name="args">Optional arguments to format <paramref name="urlBase"/></param>
    /// <returns>Complete url</returns>
    protected string GetUrl(string urlBase, int year, bool fullPlot, bool includeTomatoRating, params object[] args)
    {
      string replacedUrl = string.Format(urlBase, args);
      if (year > 0) replacedUrl += "&y=" + year.ToString();
      if(fullPlot) replacedUrl += "&plot=full";
      else replacedUrl += "&plot=short";
      if(includeTomatoRating) replacedUrl += "&tomatoes=true";
      else replacedUrl += "&tomatoes=false";
      return string.Format("{0}&r=json", replacedUrl);
    }

    /// <summary>
    /// Creates a local file name for loading and saving details for movie. It supports both TMDB id and IMDB id.
    /// </summary>
    /// <param name="movieId"></param>
    /// <param name="prefix"></param>
    /// <returns>Cache file name or <c>null</c> if directory could not be created</returns>
    protected string CreateAndGetCacheName<TE>(TE movieId, string prefix)
    {
      try
      {
        string folder = Path.Combine(_cachePath, movieId.ToString());
        if (!Directory.Exists(folder))
          Directory.CreateDirectory(folder);
        return Path.Combine(folder, string.Format("{0}.json", prefix));
      }
      catch
      {
        // TODO: logging
        return null;
      }
    }

    protected static ILogger Logger
    {
      get
      {
        return ServiceRegistration.Get<ILogger>();
      }
    }

    #endregion
  }
}
