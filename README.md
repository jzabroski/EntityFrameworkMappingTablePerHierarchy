# EntityFrameworkMappingTablePerHierarchy

This is an attempt to wrap my head around EntityFrameworkCore's Table-Per-Hierarchy (TPH) implementation, and suss out any issues in it.

Core concerns are:
- In March 2018, EntityFrameworkCore team [fixed mapping Discriminator as an enum](https://github.com/aspnet/EntityFrameworkCore/issues/11454).  This does not appear to work using fluent syntax.  The [pull request associated with the fix](https://github.com/aspnet/EntityFrameworkCore/commit/e6894d01230abec664b7461aa76464edeaada3e0) seems to be more of a unit test than integration test.
- `HasDiscriminator` is finicky. `HasDiscriminator<TEntity>(x => x.DiscriminatorEnum)` will blow up. `HasDiscriminator(x => x.DiscriminatorEnum)` won't. TODO: Understand why?
- Can I [reference a TPH twice in the same entity](https://github.com/aspnet/EntityFrameworkCore/issues/5001)?
- Can I have One TPH with a foreign key to one TPH leaf entity?
- Why are all my exceptions occurring on `IDatabaseCreator.EnsureDeleted`?

# Domain Model

This project started off as an attempt to model a specific domain model using a non-trivial Table-Per-Hierarchy.

## C# Class Diagram

```
    .--------------------------------------------.     .----------------------------------------------.
    |  ParentBase <abstract>                     |     | ChildBase <abstract>                         |
    +--------------------------------------------+     +----------------------------------------------+
    | Id : long                                  |     | Id : long                                    |
    '--------------------------------------------'     '----------------------------------------------'
        |                                     |            |                                      |
       \|/                                   \|/          \|/                                    \|/
.-------------------------. .------------------------. .------------------------. .-----------------------.
| GoodParent              | | BadParent              | | GoodChild              | | BadChild              |
+-------------------------+ +------------------------+ +------------------------+ +-----------------------+
| Child : GoodChild       | | Child : BadChild       | | Parent : GoodParent    | | Parent : BadParent    |
| GoodParentData : string | | BadParentData : string | | GoodChildData : string | | BadChildData : string |
'-------------------------' '------------------------' '------------------------' '-----------------------'
```

## Sql Schema

```
.--------------------------------.
|  Parent                        |
+--------------------------------+
| ParentID       : bigint        |
| Discriminator  : bigint        |
| GoodParentData : varchar       |
| BadParentData  : varchar       |
'--------------------------------'
                | 1
                |
                | 0..1
.--------------------------------.
|  Child                         |
+--------------------------------+
| ChildId        : bigint        |
| ParentId       : bigint        |
| Discriminator  : bigint        |
| GoodChildData  : varchar       |
| BadChildData   : varchar       |
'--------------------------------'                                                         
```

