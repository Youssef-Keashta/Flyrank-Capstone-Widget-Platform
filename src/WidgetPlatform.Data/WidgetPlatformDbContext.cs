using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WidgetPlatform.Domain;

namespace WidgetPlatform.Data
{
    public class WidgetPlatformDbContext : IdentityDbContext<ApplicationUser>
    {
        public WidgetPlatformDbContext(DbContextOptions<WidgetPlatformDbContext> options)
            : base(options) { }

        public DbSet<Widget> Widgets => Set<Widget>();
        public DbSet<Submission> Submissions => Set<Submission>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
