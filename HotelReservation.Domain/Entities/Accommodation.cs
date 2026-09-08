using HotelReservation.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HotelReservation.Domain.Entities
{
    public class Accommodation
    {

        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public AccommodationType Type { get; set; }

        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Draft;

        public string? RejectionReason { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }

        public int OwnerId { get; set; }

        public Owner Owner { get; set; } = null!;

        public ICollection<AccommodationUnit> Units { get; set; }
            = new List<AccommodationUnit>();

        public ICollection<AccommodationPhoto> Photos { get; set; }
            = new List<AccommodationPhoto>();

        public ICollection<AccommodationAmenity> AccommodationAmenities { get; set; }
            = new List<AccommodationAmenity>();

        public ICollection<Review> Reviews { get; set; }
            = new List<Review>();

        public ICollection<AvailabilityBlock> AvailabilityBlocks { get; set; }
            = new List<AvailabilityBlock>();

    }
}
