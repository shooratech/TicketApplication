﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace TicketSystemApplication
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class TicketSystemEntities : DbContext
    {
        public TicketSystemEntities()
            : base("name=TicketSystemEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<tbglTicket> tbglTickets { get; set; }
        public virtual DbSet<tbglTicket_H> tbglTicket_H { get; set; }
        public virtual DbSet<tbglUser> tbglUsers { get; set; }
        public virtual DbSet<tbWebUrl> tbWebUrls { get; set; }
    }
}