# EntityFrameworkMappingTablePerHierarchy

This is an attempt to wrap my head around EntityFrameworkCore's Table-Per-Hierarchy (TPH) implementation, and suss out any issues in it.

Core concerns are:
- In March 2018, EntityFrameworkCore team [fixed mapping Discriminator as an enum](https://github.com/aspnet/EntityFrameworkCore/issues/11454).  This does not appear to work using fluent syntax.  The associated pull request with the fix seems to be more of a unit test than integration test.
- `HasDiscriminator` is finicky. `HasDiscriminator<TEntity>(x => x.DiscriminatorEnum)` will blow up. `HasDiscriminator(x => x.DiscriminatorEnum)` won't. TODO: Understand why?
- Can I [reference a TPH twice in the same entity](https://github.com/aspnet/EntityFrameworkCore/issues/5001)?
- Can I have One TPH with a foreign key to one TPH leaf entity?
- Why are all my exceptions occurring on `IDatabaseCreator.EnsureDeleted`?
