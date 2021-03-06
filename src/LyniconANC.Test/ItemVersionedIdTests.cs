﻿using System;
using System.Collections.Generic;
using System.Linq;
using Lynicon.Attributes;
using LyniconANC.Test.Models;
using Lynicon.Collation;
using Lynicon.Extensibility;
using Lynicon.Models;
using Lynicon.Repositories;
using Newtonsoft.Json;
using Xunit;

namespace LyniconANC.Test
{
    [Collection("Lynicon System")]
    public class ItemVersionedIdTests
    {
        LyniconSystemFixture sys;

        public ItemVersionedIdTests(LyniconSystemFixture sys)
        {
            this.sys = sys;
        }

        [Fact]
        public void ItemVersionedIdEquality()
        {
            Guid id0 = Guid.NewGuid();
            ItemVersion iv0 = new ItemVersion(new Dictionary<string, object> { { "Locale", "en-GB" } });
            ItemVersion iv1 = new ItemVersion(new Dictionary<string, object> { { "Locale", "es-ES" } });
            var ii0 = new ItemVersionedId(typeof(HeaderContent), id0, iv0);
            var ii1 = new ItemVersionedId(typeof(HeaderContent), new Guid(id0.ToString()), new ItemVersion(iv0));
            var ii2 = new ItemVersionedId(typeof(RestaurantContent), id0, new ItemVersion(iv0));
            var ii3 = new ItemVersionedId(typeof(HeaderContent), Guid.NewGuid(), new ItemVersion(iv0));
            var ii4 = new ItemVersionedId(typeof(HeaderContent), new Guid(id0.ToString()), iv1);

            Assert.True(ii0.Equals(ii1), ".Equals true");
            Assert.True(ii0 == ii1, "== true");
            Assert.False(ii0.Equals(ii2), ".Equals false by type");
            Assert.False(ii0 == ii2, "== false by type");
            Assert.False(ii1.Equals(ii3), ".Equals false by id");
            Assert.False(ii1 == ii3, "== false by id");
            Assert.False(ii0.Equals(ii4), ".Equals false by version");
            Assert.False(ii0 == ii4, "== false by version");

            Assert.False(ii0.GetHashCode() == ii2.GetHashCode(), "hash code by type");
            Assert.False(ii1.GetHashCode() == ii3.GetHashCode(), "hash code by id");
            Assert.False(ii0.GetHashCode() == ii4.GetHashCode(), "hash code by version");
        }

        [Fact]
        public void ItemVersionedIdConstructors()
        {
            ItemVersion iv0 = new ItemVersion(new Dictionary<string, object> { { "Locale", "en-GB" } });

            // ItemId uses ContentType() of the relevant type
            Guid id1 = Guid.NewGuid();
            Type extType = sys.LyniconSystem.Extender[typeof(TestData)];
            var ii1 = new ItemVersionedId(extType, id1, iv0);
            Assert.Equal(typeof(TestData), ii1.Type);

            // Serialize/Deserialize
            var ii2 = new ItemVersionedId(ii1.ToString());
            Assert.Equal(ii2.Type, ii1.Type);
            Assert.Equal(ii2.Id, ii1.Id);
            Assert.Equal(ii2, ii1);

            // Construct from basic type
            TestData td = Collator.Instance.GetNew<TestData>(new Address(typeof(TestData), "a"));
            td.Id = 5;
            var ii3 = new ItemVersionedId(sys.LyniconSystem, td);
            Assert.Equal(ii3.Id, td.Id);
            Assert.Equal(ii3.Version, VersionManager.Instance.CurrentVersionForType(typeof(TestData)));

            // Construct from container
            //Guid id = Guid.NewGuid();
            //Guid ident = Guid.NewGuid();
            //ContentItem ci = new ContentItem { Id = id, Identity = ident, DataType = typeof(RestaurantContent).FullName };
            //var ii4 = new ItemVersionedId(ci);
            //Assert.Equal(ii4.Id, ident);
            //Assert.Equal(ii4.Type, typeof(RestaurantContent));
            //Assert.Equal(ii4.Version, VersionManager.Instance.CurrentVersionForType(typeof(RestaurantContent)));

            // Construct from data item
            RestaurantContent rc = Collator.Instance.GetNew<RestaurantContent>(new Address(typeof(RestaurantContent), "x"));
            var ii5 = new ItemVersionedId(sys.LyniconSystem, rc);
            Assert.Equal(ii5.Id, ((ICoreMetadata)rc).Identity);
            Assert.Equal(ii5.Version, VersionManager.Instance.CurrentVersionForType(typeof(RestaurantContent)));

            // Construct from summary
            RestaurantSummary rs = Collator.Instance.GetSummary<RestaurantSummary>(rc);
            var ii6 = new ItemVersionedId(sys.LyniconSystem, rs);
            Assert.Equal(ii6.Id, rs.Id);
            Assert.Equal(ii6.Type, rs.Type);
            Assert.Equal(ii6.Version, VersionManager.Instance.CurrentVersionForType(typeof(RestaurantContent)));

            // No empty value allowed
            ContentItem cc = null;
            ItemId iitest = null;
            object otest = null;
            string stest = null;
            Summary summtest = null;
            Assert.Throws<NullReferenceException>(() => new ItemVersionedId(sys.LyniconSystem, cc));
            Assert.Throws<NullReferenceException>(() => new ItemVersionedId(iitest));
            Assert.Throws<NullReferenceException>(() => new ItemVersionedId(sys.LyniconSystem, otest));
            Assert.Throws<ArgumentException>(() => new ItemVersionedId(stest));
            Assert.Throws<NullReferenceException>(() => new ItemVersionedId(sys.LyniconSystem, summtest));
            Assert.Throws<ArgumentException>(() => new ItemVersionedId(typeof(RestaurantContent), null, iv0));
            Assert.Throws<ArgumentException>(() => new ItemVersionedId(null, Guid.NewGuid(), iv0));
            Assert.Throws<ArgumentException>(() => new ItemVersionedId(typeof(RestaurantContent), Guid.NewGuid(), null));
        }

        [Fact]
        public void ItemIdSerialization()
        {
            Guid id0 = Guid.NewGuid();
            ItemVersion iv0 = new ItemVersion(new Dictionary<string, object> { { "Locale", "en-GB" } });
            var ii0 = new ItemVersionedId(typeof(TestData), id0, iv0);
            var dict = new Dictionary<ItemVersionedId, string>();
            dict.Add(ii0, "hello");

            string ser = JsonConvert.SerializeObject(dict);
            var dictOut = JsonConvert.DeserializeObject<Dictionary<ItemVersionedId, string>>(ser);

            Assert.Equal("hello", dictOut[ii0]);

            ser = JsonConvert.SerializeObject(ii0);
            var iiOut = JsonConvert.DeserializeObject<ItemVersionedId>(ser);

            Assert.Equal(ii0, iiOut);
        }
    }
}
