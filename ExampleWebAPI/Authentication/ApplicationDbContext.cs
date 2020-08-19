using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExampleWebAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace ExampleWebAPI.Authentication {
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {

        
        }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);

            // reads sample data from csv file
            List<Event> sampleDataEvents = new List<Event>();
            int nextId = 1;
            bool firstLine = true;
            StreamReader streamReader = new StreamReader("TestData/sample_data.csv");
            while (!streamReader.EndOfStream) {

                string line = streamReader.ReadLine();
                // skips the line containing header
                if (firstLine) {
                    firstLine = false;
                    continue;
                }

                string[] values = line.Split(',');

                sampleDataEvents.Add(
                    new Event {
                        Id = int.Parse(values[0]),
                        CampaignName = values[1],
                        EventType = values[2],
                        AppUserId = values[3],
                        AppUserGender = values[4],
                        EventDate = DateTime.Parse(values[5]),
                        AppDeviceType = values[6],
                    }
                );
                nextId++;
            }

            // adds list of sample data
            builder.Entity<Event>().HasData(sampleDataEvents);
        }

        public DbSet<Event> Events { get; set; }
    }
}

