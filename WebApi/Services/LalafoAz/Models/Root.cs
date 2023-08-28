using System.Collections.Generic;

namespace WebApi.Services.LalafoAz.Models
{
    public class InitialState
    {
        public Listing listing { get; set; }
    }

    public class Item
    {
        public int id { get; set; }
        public long created_time { get; set; }
    }
    public class AdDetails
    {

    }
    public class Listing
    {
        public ListingFeed listingFeed { get; set; }
    }

    public class ListingFeed
    {
        public List<Item> items { get; set; }
    }

    public class Props
    {
        public InitialState initialState { get; set; }
    }

    public class Root
    {
        public Props props { get; set; }
    }
}
