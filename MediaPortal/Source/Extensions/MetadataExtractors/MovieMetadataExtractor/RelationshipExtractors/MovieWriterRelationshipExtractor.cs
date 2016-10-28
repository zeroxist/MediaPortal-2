﻿#region Copyright (C) 2007-2015 Team MediaPortal

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

using System;
using System.Collections.Generic;
using System.Linq;
using MediaPortal.Common;
using MediaPortal.Common.Logging;
using MediaPortal.Common.MediaManagement;
using MediaPortal.Common.MediaManagement.DefaultItemAspects;
using MediaPortal.Common.MediaManagement.Helpers;
using MediaPortal.Extensions.OnlineLibraries;
using MediaPortal.Common.MediaManagement.MLQueries;

namespace MediaPortal.Extensions.MetadataExtractors.MovieMetadataExtractor
{
  class MovieWriterRelationshipExtractor : IMovieRelationshipExtractor, IRelationshipRoleExtractor
  {
    private static readonly Guid[] ROLE_ASPECTS = { MovieAspect.ASPECT_ID, VideoAspect.ASPECT_ID };
    private static readonly Guid[] LINKED_ROLE_ASPECTS = { PersonAspect.ASPECT_ID };

    public bool BuildRelationship
    {
      get { return true; }
    }

    public Guid Role
    {
      get { return MovieAspect.ROLE_MOVIE; }
    }

    public Guid[] RoleAspects
    {
      get { return ROLE_ASPECTS; }
    }

    public Guid LinkedRole
    {
      get { return PersonAspect.ROLE_WRITER; }
    }

    public Guid[] LinkedRoleAspects
    {
      get { return LINKED_ROLE_ASPECTS; }
    }

    public IFilter GetSearchFilter(IDictionary<Guid, IList<MediaItemAspect>> extractedAspects)
    {
      return GetPersonSearchFilter(extractedAspects);
    }

    public bool TryExtractRelationships(IDictionary<Guid, IList<MediaItemAspect>> aspects, out IDictionary<IDictionary<Guid, IList<MediaItemAspect>>, Guid> extractedLinkedAspects, bool forceQuickMode)
    {
      extractedLinkedAspects = null;

      if (!MovieMetadataExtractor.IncludeWriterDetails)
        return false;

      if (forceQuickMode)
        return false;

      if (BaseInfo.IsVirtualResource(aspects))
        return false;

      MovieInfo movieInfo = new MovieInfo();
      if (!movieInfo.FromMetadata(aspects))
        return false;

      if (!AddToCheckCache(movieInfo))
        return false;

      if(!MovieMetadataExtractor.SkipOnlineSearches)
        OnlineMatcherService.Instance.UpdatePersons(movieInfo, PersonAspect.OCCUPATION_WRITER, forceQuickMode);

      if (movieInfo.Writers.Count == 0)
        return false;

      if (BaseInfo.CountRelationships(aspects, LinkedRole) < movieInfo.Writers.Count)
        movieInfo.HasChanged = true; //Force save if no relationship exists

      if (!movieInfo.HasChanged && !forceQuickMode)
        return false;

      extractedLinkedAspects = new Dictionary<IDictionary<Guid, IList<MediaItemAspect>>, Guid>();

      foreach (PersonInfo person in movieInfo.Writers)
      {
        person.AssignNameId();
        person.HasChanged = movieInfo.HasChanged;
        IDictionary<Guid, IList<MediaItemAspect>> personAspects = new Dictionary<Guid, IList<MediaItemAspect>>();
        person.SetMetadata(personAspects);

        if (personAspects.ContainsKey(ExternalIdentifierAspect.ASPECT_ID))
        {
          Guid existingId;
          if (TryGetIdFromWriterCache(person, out existingId))
            extractedLinkedAspects.Add(personAspects, existingId);
          else
            extractedLinkedAspects.Add(personAspects, Guid.Empty);
        }
      }
      return extractedLinkedAspects.Count > 0;
    }

    public bool TryMatch(IDictionary<Guid, IList<MediaItemAspect>> extractedAspects, IDictionary<Guid, IList<MediaItemAspect>> existingAspects)
    {
      if (!existingAspects.ContainsKey(PersonAspect.ASPECT_ID))
        return false;

      PersonInfo linkedPerson = new PersonInfo();
      if (!linkedPerson.FromMetadata(extractedAspects))
        return false;

      PersonInfo existingPerson = new PersonInfo();
      if (!existingPerson.FromMetadata(existingAspects))
        return false;

      return linkedPerson.Equals(existingPerson);
    }

    public bool TryGetRelationshipIndex(IDictionary<Guid, IList<MediaItemAspect>> aspects, IDictionary<Guid, IList<MediaItemAspect>> linkedAspects, out int index)
    {
      index = -1;

      SingleMediaItemAspect linkedAspect;
      if (!MediaItemAspect.TryGetAspect(linkedAspects, PersonAspect.Metadata, out linkedAspect))
        return false;

      string name = linkedAspect.GetAttributeValue<string>(PersonAspect.ATTR_PERSON_NAME);

      SingleMediaItemAspect aspect;
      if (!MediaItemAspect.TryGetAspect(aspects, VideoAspect.Metadata, out aspect))
        return false;

      IEnumerable<object> persons = aspect.GetCollectionAttribute<object>(VideoAspect.ATTR_WRITERS);
      List<string> nameList = new List<string>(persons.Cast<string>());

      index = nameList.IndexOf(name);
      return index >= 0;
    }

    public void CacheExtractedItem(Guid extractedItemId, IDictionary<Guid, IList<MediaItemAspect>> extractedAspects)
    {
      PersonInfo person = new PersonInfo();
      person.FromMetadata(extractedAspects);
      AddToWriterCache(extractedItemId, person);
    }

    internal static ILogger Logger
    {
      get { return ServiceRegistration.Get<ILogger>(); }
    }
  }
}
