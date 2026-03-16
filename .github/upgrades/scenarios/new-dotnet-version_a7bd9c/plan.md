# .NET 10.0 Upgrade Plan

## Table of Contents

- [Executive Summary](#executive-summary)
- [Migration Strategy](#migration-strategy)
- [Detailed Dependency Analysis](#detailed-dependency-analysis)
- [Implementation Timeline](#implementation-timeline)
- [Project-by-Project Migration Plans](#project-by-project-migration-plans)
  - [Bot.Model](#botmodel)
  - [Bot.Services](#botservices)
  - [Bot.Console](#botconsole)
- [Package Update Reference](#package-update-reference)
- [Breaking Changes Catalog](#breaking-changes-catalog)
- [Risk Management](#risk-management)
- [Testing & Validation Strategy](#testing--validation-strategy)
- [Complexity & Effort Assessment](#complexity--effort-assessment)
- [Source Control Strategy](#source-control-strategy)
- [Success Criteria](#success-criteria)

---

## Executive Summary

### Scenario Description

Upgrade the RickrollBot solution from .NET Framework 4.7.2 to .NET 10.0 (Long Term Support), modernizing all three projects and their dependencies to leverage the latest .NET capabilities, improved performance, and long-term support lifecycle.

### Scope

**Projects Affected:**
- Bot.Model (ClassLibrary) - 673 LOC, 0 dependencies
- Bot.Services (ClassLibrary) - 5,390 LOC, 1 dependency, **141 API issues**
- Bot.Console (Application) - 116 LOC, 2 dependencies

**Current State:** All projects targeting .NET Framework 4.7.2 (net472), SDK-style projects

**Target State:** All projects targeting .NET 10.0 (net10.0)

### Selected Strategy

**All-At-Once Strategy** - All projects upgraded simultaneously in single coordinated operation.

**Rationale:**
- Small solution (3 projects) enables atomic upgrade
- All projects currently on same framework version (net472)
- Clear, linear dependency structure (no circular dependencies)
- SDK-style projects already in place (simplified upgrade)
- **User requirement:** Include all NuGet package updates, allow beta releases for Microsoft.Skype.Bots.Media

### Complexity Assessment

**Discovered Metrics:**
- Total Projects: 3
- Total NuGet Packages: 27 (17 need upgrade, 10 incompatible)
- Total LOC: 6,179
- Estimated LOC Impact: 141+ lines (2.3% of codebase)
- API Compatibility Issues: 141 total (82 binary incompatible, 23 source incompatible, 36 behavioral changes)
- Files with Incidents: 17 out of 55 files

**Complexity Classification:**
- **Bot.Model:** 🟢 Low - No API issues, minimal package updates
- **Bot.Services:** 🟡 Medium - **Primary complexity driver** with ASP.NET Framework → ASP.NET Core migration
- **Bot.Console:** 🟢 Low - No API issues, minimal package updates

### Critical Issues

**Security & Deprecation Concerns:**
1. **Microsoft.ApplicationInsights.TraceListener (2.20.0)** - Deprecated package
2. **Microsoft.ApplicationInsights.WorkerService (2.20.0)** - Deprecated package
3. **Microsoft.IdentityModel.Clients.ActiveDirectory (5.2.9)** - Deprecated, migrate to Microsoft.Identity.Client

**Major Breaking Changes:**
1. **ASP.NET Framework → ASP.NET Core:** 76 API incompatibilities in System.Web.Http namespace
   - ApiController class and routing attributes
   - HttpRequestMessage extension methods
   - OWIN middleware integration
2. **JWT Token Validation:** System.IdentityModel.Tokens.Jwt API changes
3. **Package Incompatibilities:** 10 packages require replacement or updates

### Recommended Approach

**All-At-Once atomic upgrade** with single commit strategy:
1. Update all project target frameworks simultaneously (net472 → net10.0)
2. Update/replace all package references in coordinated batch
3. Address breaking changes in Bot.Services (ASP.NET Core migration)
4. Build solution and fix compilation errors
5. Validate through comprehensive testing

**Expected Timeline Structure:**
- **Phase 0:** Prerequisites (SDK validation, branch setup)
- **Phase 1:** Atomic upgrade operation (projects + packages + breaking changes)
- **Phase 2:** Testing and validation

### Iteration Strategy

**Fast Batch Approach** (2-3 detail iterations):
- Iteration 2.x: Foundation (dependency analysis, strategy details, project stubs)
- Iteration 3.1: Low-complexity projects batch (Bot.Model + Bot.Console)
- Iteration 3.2: Medium-complexity project (Bot.Services with ASP.NET Core migration)
- Final iteration: Success criteria and source control

---

## Migration Strategy

### Approach Selection

**Selected: All-At-Once Strategy**

All three projects will be upgraded simultaneously in a single atomic operation, updating target frameworks, packages, and code in one coordinated batch.

### Justification

**Why All-At-Once is Optimal:**

1. **Small Solution Size:** Only 3 projects makes atomic upgrade manageable
   - Entire codebase is 6,179 LOC (medium-small)
   - Changes concentrated in Bot.Services (87% of impact)

2. **Homogeneous Starting Point:** All projects on net472
   - No multi-targeting needed
   - No intermediate framework versions to maintain
   - Clean upgrade path without framework mixing

3. **Clear Dependency Structure:** Linear DAG without cycles
   - No complex interdependencies
   - Predictable build order
   - Low risk of dependency resolution conflicts

4. **SDK-Style Projects:** Already modernized project format
   - Simplified target framework changes
   - Streamlined package management
   - Reduced manual XML editing

5. **User Requirements:** Comprehensive package updates requested
   - All packages should be updated in single pass
   - Beta versions acceptable for Microsoft.Skype.Bots.Media
   - Atomic approach aligns with complete modernization goal

6. **Risk vs Speed Trade-off:** Faster completion outweighs incremental safety
   - Medium overall complexity (concentrated in one project)
   - Breaking changes are well-documented (ASP.NET Core migration)
   - Comprehensive testing can validate entire solution at once

**Alternative Considered: Incremental Migration**
- ❌ Unnecessary overhead for small solution
- ❌ Would require multi-targeting or intermediate .NET versions
- ❌ Longer timeline without meaningful risk reduction
- ❌ More complex source control with multiple phases

### All-At-Once Strategy Rationale

**Key Characteristics:**
- **Single Upgrade Phase:** All projects move from net472 → net10.0 together
- **Unified Package Updates:** All 17 package upgrades applied simultaneously
- **Coordinated Breaking Changes:** ASP.NET Framework → ASP.NET Core migration happens atomically
- **No Intermediate States:** Solution either fully on .NET Framework 4.7.2 or fully on .NET 10.0

**Benefits for This Solution:**
- Fastest path to .NET 10.0 LTS
- No compatibility shims or multi-targeting complexity
- Single comprehensive test cycle
- Clean source control history (single upgrade commit)
- All projects benefit from .NET 10 immediately

**Risk Mitigation:**
- Comprehensive breaking changes catalog (see Breaking Changes section)
- Bottom-up validation sequence (Model → Services → Console)
- Pre-upgrade SDK verification
- Rollback plan via source control branch

### Dependency-Based Ordering Principles

While all projects upgrade simultaneously, **validation and testing** follow dependency order:

**Build Order (MSBuild automatic):**
1. Bot.Model (no dependencies)
2. Bot.Services (depends on Bot.Model)
3. Bot.Console (depends on Bot.Services)

**Validation Sequence:**
1. ✅ Bot.Model builds and tests pass
2. ✅ Bot.Services builds (consuming upgraded Bot.Model) and tests pass
3. ✅ Bot.Console builds (consuming upgraded stack) and application runs

This ensures that if issues arise, they're identified at the appropriate dependency level.

### Parallel vs Sequential Execution

**During Upgrade (All-At-Once):**
- All `.csproj` files edited simultaneously
- All `PackageReference` updates applied together
- Breaking changes addressed in single pass

**During Validation:**
- **Sequential:** Build and test from bottom to top (dependency order)
- Allows early detection of propagating issues
- Failed validation at lower level blocks higher levels

### Phase Definitions

#### Phase 0: Prerequisites
**Duration:** Quick validation checks
- Verify .NET 10.0 SDK installed
- Confirm on correct source control branch (`upgrade-to-NET10`)
- Validate baseline (current solution builds on net472)

#### Phase 1: Atomic Upgrade
**Scope:** All projects simultaneously
- Update `TargetFramework` property: `net472` → `net10.0` (all 3 projects)
- Update all package references (17 packages)
- Replace incompatible packages (10 packages)
- Remove framework-included packages (2 packages)
- Address breaking changes (primarily Bot.Services)
- Build solution and fix compilation errors
- Solution builds with 0 errors

**Key Operations:**
1. Project file modifications (3 files)
2. Package reference updates (27 packages across projects)
3. Code modifications for ASP.NET Core migration (Bot.Services)
4. JWT authentication modernization (Bot.Services)
5. OWIN → ASP.NET Core middleware (Bot.Services)

#### Phase 2: Testing & Validation
**Scope:** Comprehensive solution validation
- Run all unit tests (if present)
- Integration testing
- Application smoke tests
- Performance validation
- Security verification

**Success Criteria:** All tests pass, application functions correctly

---

## Detailed Dependency Analysis

### Dependency Graph Summary

The solution has a clean, linear dependency structure with no circular dependencies:

```
Bot.Model (leaf - no dependencies)
    ↑
Bot.Services (depends on Bot.Model)
    ↑
Bot.Console (depends on Bot.Model + Bot.Services)
```

**Dependency Characteristics:**
- **Depth:** 2 levels maximum
- **Leaf Node:** Bot.Model (0 dependencies, 2 dependants)
- **Intermediate Node:** Bot.Services (1 dependency, 1 dependant)
- **Root Node:** Bot.Console (2 dependencies, 0 dependants)
- **Circular Dependencies:** None

### Project Groupings for All-At-Once Strategy

Since we're using the All-At-Once strategy, all projects will be upgraded simultaneously. However, understanding the dependency order is critical for:
1. **Validation sequence** - Test from bottom-up (Model → Services → Console)
2. **Breaking change propagation** - Changes in Bot.Model may affect Bot.Services and Bot.Console
3. **Build order** - MSBuild will naturally build Bot.Model first

**Logical Grouping (for understanding, not sequential execution):**

| Group | Projects | Rationale |
|-------|----------|-----------|
| **Foundation** | Bot.Model | Leaf node with no dependencies; changes here impact downstream projects |
| **Core Services** | Bot.Services | Contains main business logic and API endpoints; depends on Bot.Model |
| **Application** | Bot.Console | Entry point application; depends on entire stack |

**All projects will be upgraded atomically in Phase 1.**

### Critical Path Identification

The **critical path** for this upgrade is:

**Bot.Services** → This project drives complexity due to:
1. ASP.NET Framework to ASP.NET Core migration (76 API incompatibilities)
2. Largest codebase (5,390 LOC with 141 API issues)
3. Most package incompatibilities (10 incompatible packages)
4. OWIN middleware requiring ASP.NET Core equivalent
5. JWT authentication modernization

Bot.Model and Bot.Console have minimal complexity and will upgrade smoothly once Bot.Services breaking changes are resolved.

### Circular Dependency Analysis

**Result:** ✅ No circular dependencies detected

The dependency graph is a clean Directed Acyclic Graph (DAG), which simplifies the upgrade process and reduces risk.

---

## Implementation Timeline

### Phase 0: Prerequisites (Pre-Flight Checks)

**Objective:** Validate environment readiness before upgrade begins

**Operations:**
- Verify .NET 10.0 SDK installation
- Confirm current branch is `upgrade-to-NET10`
- Validate baseline build (solution builds on net472 without errors)
- Backup/commit current state

**Deliverables:**
- ✅ .NET 10.0 SDK available
- ✅ On correct branch
- ✅ Clean baseline build
- ✅ Source control checkpoint

**Estimated Effort:** Low complexity

---

### Phase 1: Atomic Upgrade (All Projects Simultaneously)

**Objective:** Upgrade entire solution from net472 to net10.0 in single coordinated operation

**Operations** (performed as single coordinated batch):

1. **Update Project Target Frameworks**
   - Bot.Model: `net472` → `net10.0`
   - Bot.Services: `net472` → `net10.0`
   - Bot.Console: `net472` → `net10.0`

2. **Update Package References** (see §Package Update Reference)
   - 17 packages require version updates
   - 10 incompatible packages require replacement
   - 2 packages removed (framework-included)
   - 10 packages remain compatible

3. **Address Breaking Changes** (see §Breaking Changes Catalog)
   - ASP.NET Framework → ASP.NET Core migration (Bot.Services)
   - JWT authentication modernization (Bot.Services)
   - OWIN → ASP.NET Core middleware (Bot.Services)
   - System.Uri behavioral changes
   - HttpContent behavioral changes

4. **Build Solution and Fix Compilation Errors**
   - Restore dependencies (`dotnet restore`)
   - Build solution (`dotnet build`)
   - Address compilation errors iteratively
   - Focus on Bot.Services (primary complexity)

5. **Verify Build Success**
   - Solution builds with 0 errors
   - No warnings related to deprecated APIs
   - Dependency resolution successful

**Deliverables:**
- ✅ All 3 projects targeting net10.0
- ✅ All package references updated
- ✅ Breaking changes resolved
- ✅ Solution builds successfully (0 errors)

**Estimated Effort:** Medium complexity (concentrated in Bot.Services)

---

### Phase 2: Testing & Validation

**Objective:** Verify functional correctness and performance of upgraded solution

**Operations:**

1. **Unit Testing**
   - Run all unit tests (if test projects exist)
   - Verify test framework compatibility
   - Address test failures

2. **Integration Testing**
   - Test Bot.Services API endpoints
   - Verify Bot.Console application startup
   - Test cross-project communication

3. **Security Validation**
   - Verify JWT authentication works correctly
   - Test Graph API connectivity
   - Validate media handling (Skype Bot Media)

4. **Performance Baseline**
   - Compare startup time
   - Memory usage patterns
   - API response times

**Deliverables:**
- ✅ All tests pass
- ✅ Application functions correctly
- ✅ No security regressions
- ✅ Performance acceptable or improved

**Estimated Effort:** Low-Medium complexity

---

### Timeline Summary

| Phase | Scope | Key Milestone |
|-------|-------|--------------|
| **Phase 0** | Prerequisites | Environment ready |
| **Phase 1** | Atomic Upgrade | Solution builds on net10.0 |
| **Phase 2** | Testing | All tests pass, app functional |

**Note:** Phases represent logical organization, not separate tasks. Phase 1 is executed as a single atomic operation per All-At-Once strategy.

---

## Project-by-Project Migration Plans

The following sections provide detailed migration specifications for each project. While the All-At-Once strategy means these changes happen simultaneously, individual project details are provided for clarity and validation purposes.

---

### Bot.Model

#### Project Overview

**Type:** ClassLibrary  
**Current Target Framework:** net472  
**Proposed Target Framework:** net10.0  
**Project Kind:** ClassLibrary, SDK-style  
**Complexity:** 🟢 Low  

**Metrics:**
- Lines of Code: 673
- Files: 14 total
- Files with Incidents: 1
- Dependencies: 0 project dependencies
- Dependants: 2 (Bot.Services, Bot.Console)
- NuGet Packages: 3

**Role in Solution:**
Bot.Model is the **foundation/leaf node** of the dependency graph. It defines core data models and contracts used by both Bot.Services and Bot.Console. Being a leaf node with no dependencies makes it the safest project to upgrade.

---

#### Current State

**Target Framework:**
```xml
<TargetFramework>net472</TargetFramework>
```

**Package References:**
- Microsoft.CSharp 4.7.0 ✅ Compatible (no update needed)
- Microsoft.Graph 4.16.0 ✅ Compatible (no update needed)
- Microsoft.Graph.Communications.Calls 1.2.0.3742 ✅ Compatible (no update needed)
- Microsoft.Owin.Host.HttpListener 4.2.0 ⚠️ **Incompatible** (requires replacement)
- Newtonsoft.Json 13.0.1 → **13.0.4** (security update recommended)
- System.Data.DataSetExtensions 4.5.0 → **Remove** (included in framework)

**API Compatibility:**
- ✅ 0 binary incompatibilities
- ✅ 0 source incompatibilities
- ✅ 0 behavioral changes
- ✅ 240 compatible APIs

**Assessment:** Bot.Model has **no API compatibility issues**, making it straightforward to upgrade.

---

#### Target State

**Target Framework:**
```xml
<TargetFramework>net10.0</TargetFramework>
```

**Updated Package References:**
- Microsoft.CSharp 4.7.0 (no change - compatible)
- Microsoft.Graph 4.16.0 (no change - compatible)
- Microsoft.Graph.Communications.Calls 1.2.0.3742 (no change - compatible)
- ~~Microsoft.Owin.Host.HttpListener~~ → **Remove** (not needed in .NET 10, OWIN replaced by ASP.NET Core)
- Newtonsoft.Json → **13.0.4**
- ~~System.Data.DataSetExtensions~~ → **Remove** (included in framework)

---

#### Migration Steps

##### 1. Prerequisites
- ✅ Bot.Model is a leaf node (no dependencies to migrate first)
- ✅ No circular dependencies

##### 2. Update Project File

**File:** `Bot.Model\Bot.Model.csproj`

**Change TargetFramework property:**
```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <!-- Other properties remain unchanged -->
</PropertyGroup>
```

##### 3. Update Package References

**File:** `Bot.Model\Bot.Model.csproj`

**Remove incompatible/unnecessary packages:**
```xml
<!-- REMOVE these lines: -->
<PackageReference Include="Microsoft.Owin.Host.HttpListener" Version="4.2.0" />
<PackageReference Include="System.Data.DataSetExtensions" Version="4.5.0" />
```

**Update Newtonsoft.Json:**
```xml
<!-- UPDATE from Version="13.0.1" to: -->
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
```

**Keep unchanged (already compatible):**
```xml
<PackageReference Include="Microsoft.CSharp" Version="4.7.0" />
<PackageReference Include="Microsoft.Graph" Version="4.16.0" />
<PackageReference Include="Microsoft.Graph.Communications.Calls" Version="1.2.0.3742" />
```

##### 4. Expected Breaking Changes

**✅ None expected** - Bot.Model has 0 API compatibility issues.

**Considerations:**
- Microsoft.Owin.Host.HttpListener removal: If Bot.Model code uses OWIN types, those references will fail at compile time (unlikely for a model library)
- System.Data.DataSetExtensions removal: DataSet extension methods now built into framework

##### 5. Code Modifications

**Expected:** None required

**Potential Review Areas:**
- If any OWIN types were used (highly unlikely in model project), remove references
- Verify no direct dependencies on System.Data.DataSetExtensions APIs

##### 6. Testing Strategy

**Unit Tests:**
- If Bot.Model has unit tests, run after upgrade
- Verify model serialization/deserialization (Newtonsoft.Json update)
- Test any Graph API model contracts

**Integration Tests:**
- Test Bot.Model consumption by Bot.Services (Phase 1)
- Test Bot.Model consumption by Bot.Console (Phase 1)

##### 7. Validation Checklist

- [ ] Project file TargetFramework updated to `net10.0`
- [ ] Newtonsoft.Json updated to `13.0.4`
- [ ] Microsoft.Owin.Host.HttpListener removed
- [ ] System.Data.DataSetExtensions removed
- [ ] Project builds without errors: `dotnet build Bot.Model\Bot.Model.csproj`
- [ ] Project builds without warnings
- [ ] No package dependency conflicts
- [ ] All tests pass (if tests exist)
- [ ] Bot.Services successfully references upgraded Bot.Model
- [ ] Bot.Console successfully references upgraded Bot.Model

---

### Bot.Console

#### Project Overview

**Type:** Application (DotNetCoreApp)  
**Current Target Framework:** net472  
**Proposed Target Framework:** net10.0  
**Project Kind:** DotNetCoreApp, SDK-style  
**Complexity:** 🟢 Low  

**Metrics:**
- Lines of Code: 116
- Files: 3 total
- Files with Incidents: 1
- Dependencies: 2 project dependencies (Bot.Model, Bot.Services)
- Dependants: 0 (root application)
- NuGet Packages: 2

**Role in Solution:**
Bot.Console is the **application entry point** and root node of the dependency graph. It orchestrates Bot.Services and Bot.Model. As the root node, it's upgraded last (after its dependencies).

---

#### Current State

**Target Framework:**
```xml
<TargetFramework>net472</TargetFramework>
```

**Package References:**
- Microsoft.CSharp 4.7.0 ✅ Compatible (no update needed)
- Microsoft.Extensions.FileProviders.Abstractions 6.0.0 → **10.0.5** (recommended update)
- System.Data.DataSetExtensions 4.5.0 → **Remove** (included in framework)

**Project References:**
- Bot.Model
- Bot.Services

**API Compatibility:**
- ✅ 0 binary incompatibilities
- ✅ 0 source incompatibilities
- ✅ 0 behavioral changes
- ✅ 85 compatible APIs

**Assessment:** Bot.Console has **no API compatibility issues** and minimal package updates.

---

#### Target State

**Target Framework:**
```xml
<TargetFramework>net10.0</TargetFramework>
```

**Updated Package References:**
- Microsoft.CSharp 4.7.0 (no change - compatible)
- Microsoft.Extensions.FileProviders.Abstractions → **10.0.5**
- ~~System.Data.DataSetExtensions~~ → **Remove** (included in framework)

**Project References:**
- Bot.Model (upgraded to net10.0)
- Bot.Services (upgraded to net10.0)

---

#### Migration Steps

##### 1. Prerequisites
- ✅ Bot.Model must be upgraded first (dependency)
- ✅ Bot.Services must be upgraded first (dependency)
- Bot.Console upgrades **last** in validation sequence

##### 2. Update Project File

**File:** `Bot.Console\Bot.Console.csproj`

**Change TargetFramework property:**
```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net10.0</TargetFramework>
  <!-- Other properties remain unchanged -->
</PropertyGroup>
```

##### 3. Update Package References

**File:** `Bot.Console\Bot.Console.csproj`

**Remove framework-included package:**
```xml
<!-- REMOVE this line: -->
<PackageReference Include="System.Data.DataSetExtensions" Version="4.5.0" />
```

**Update Microsoft.Extensions.FileProviders.Abstractions:**
```xml
<!-- UPDATE from Version="6.0.0" to: -->
<PackageReference Include="Microsoft.Extensions.FileProviders.Abstractions" Version="10.0.5" />
```

**Keep unchanged:**
```xml
<PackageReference Include="Microsoft.CSharp" Version="4.7.0" />
```

##### 4. Expected Breaking Changes

**✅ None expected** - Bot.Console has 0 API compatibility issues.

**Considerations:**
- System.Data.DataSetExtensions removal: DataSet APIs now in framework
- Microsoft.Extensions.FileProviders.Abstractions update: Minor version bump (6.0 → 10.0), API stable

##### 5. Code Modifications

**Expected:** None required

**Potential Review Areas:**
- Verify application entry point (`Program.cs` or `Main` method) works correctly
- If using file providers, ensure compatible with updated abstractions package
- Verify dependency injection setup (if using `IServiceCollection`)

##### 6. Testing Strategy

**Application Testing:**
- Build and run Bot.Console application
- Verify startup sequence
- Test Bot.Services integration
- Test Bot.Model data flow

**Integration Tests:**
- End-to-end application workflow
- Verify all injected services resolve correctly
- Test external integrations (Graph API, Skype Bot Media)

**Smoke Tests:**
- Application starts without errors
- Configuration loads correctly
- Core functionality accessible

##### 7. Validation Checklist

- [ ] Project file TargetFramework updated to `net10.0`
- [ ] Microsoft.Extensions.FileProviders.Abstractions updated to `10.0.5`
- [ ] System.Data.DataSetExtensions removed
- [ ] Project builds without errors: `dotnet build Bot.Console\Bot.Console.csproj`
- [ ] Project builds without warnings
- [ ] No package dependency conflicts
- [ ] Project references to Bot.Model and Bot.Services resolve correctly
- [ ] Application runs without errors: `dotnet run --project Bot.Console\Bot.Console.csproj`
- [ ] Application startup completes successfully
- [ ] Core functionality works (basic smoke test)
- [ ] No runtime exceptions during initialization

---

### Bot.Services

#### Project Overview

**Type:** ClassLibrary  
**Current Target Framework:** net472  
**Proposed Target Framework:** net10.0  
**Project Kind:** ClassLibrary, SDK-style  
**Complexity:** 🟡 Medium  

**Metrics:**
- Lines of Code: 5,390 (87% of solution)
- Files: 39 total
- Files with Incidents: 15 (38% of files)
- Dependencies: 1 project dependency (Bot.Model)
- Dependants: 1 (Bot.Console)
- NuGet Packages: 25

**Role in Solution:**
Bot.Services is the **core business logic layer** containing API endpoints, authentication, and Bot Framework integration. It's the **primary complexity driver** for this upgrade due to ASP.NET Framework → ASP.NET Core migration.

---

#### Current State

**Target Framework:**
```xml
<TargetFramework>net472</TargetFramework>
```

**Package References (25 total):**

**ASP.NET Web API (5 packages - INCOMPATIBLE):**
- Microsoft.AspNet.WebApi 5.2.7 ⚠️ Incompatible
- Microsoft.AspNet.WebApi.Owin 5.2.7 ⚠️ Incompatible

**OWIN Packages (4 packages - INCOMPATIBLE):**
- Microsoft.Owin.Cors 4.2.0 ⚠️ Incompatible
- Microsoft.Owin.Host.HttpListener 4.2.0 ⚠️ Incompatible
- Microsoft.Owin.Hosting 4.2.0 ⚠️ Incompatible
- Microsoft.Owin.StaticFiles 4.2.0 ⚠️ Incompatible

**Microsoft.Extensions (5 packages - UPDATE RECOMMENDED):**
- Microsoft.Extensions.Configuration.EnvironmentVariables 6.0.0 → 10.0.5
- Microsoft.Extensions.Configuration.Json 6.0.0 → 10.0.5
- Microsoft.Extensions.DependencyInjection 6.0.0 → 10.0.5
- Microsoft.Extensions.Options.ConfigurationExtensions 6.0.0 → 10.0.5
- System.Diagnostics.DiagnosticSource 6.0.0 → 10.0.5

**Authentication (1 package - DEPRECATED):**
- Microsoft.IdentityModel.Clients.ActiveDirectory 5.2.9 ⚠️ Deprecated → Replace with Microsoft.Identity.Client

**Application Insights (2 packages - DEPRECATED):**
- Microsoft.ApplicationInsights.TraceListener 2.20.0 ⚠️ Deprecated
- Microsoft.ApplicationInsights.WorkerService 2.20.0 ⚠️ Deprecated

**Graph/Bot Framework (3 packages):**
- Microsoft.Graph 4.16.0 ✅ Compatible
- Microsoft.Graph.Communications.Calls 1.2.0.3742 ✅ Compatible
- Microsoft.Graph.Communications.Calls.Media 1.2.0.3742 → 1.2.0.15690 (updated version available)
- Microsoft.Skype.Bots.Media 1.21.0.241-alpha ⚠️ Incompatible (beta version needed for .NET 10)

**Utilities (5 packages):**
- DotNetEnv 2.3.0 ✅ Compatible
- NAudio 2.0.1 ✅ Compatible
- Newtonsoft.Json 13.0.1 → 13.0.4 (security update)
- Newtonsoft.Json.Bson 1.0.2 ✅ Compatible
- SharpZipLib 1.3.3 ✅ Compatible
- Microsoft.NETCore.Platforms 6.0.1 → Remove (framework-included)

**Project References:**
- Bot.Model

**API Compatibility Issues (141 total):**
- 🔴 82 binary incompatibilities
- 🟡 23 source incompatibilities
- 🔵 36 behavioral changes

**Primary Issue Categories:**
1. **ASP.NET Framework (System.Web.Http):** 76 incompatibilities
   - ApiController class
   - Routing attributes ([Route], [HttpGet], [HttpPost], etc.)
   - HttpRequestMessage extension methods
   - Request.CreateResponse patterns

2. **Identity/JWT (System.IdentityModel):** 3 incompatibilities
   - JwtSecurityTokenHandler API changes
   - Token validation parameter changes

3. **Behavioral Changes:** 36 instances
   - System.Uri constructor/property behaviors
   - HttpContent handling differences

---

#### Target State

**Target Framework:**
```xml
<TargetFramework>net10.0</TargetFramework>
```

**Updated Package References:**

**ASP.NET Core Replacements:**
- ~~Microsoft.AspNet.WebApi~~ → **Remove** (replace with ASP.NET Core)
- ~~Microsoft.AspNet.WebApi.Owin~~ → **Remove** (replace with ASP.NET Core)
- **Add:** Microsoft.AspNetCore.App (framework reference - implicit)

**OWIN Replacements:**
- ~~Microsoft.Owin.Cors~~ → **Remove** (use ASP.NET Core CORS)
- ~~Microsoft.Owin.Host.HttpListener~~ → **Remove** (use Kestrel)
- ~~Microsoft.Owin.Hosting~~ → **Remove** (use ASP.NET Core hosting)
- ~~Microsoft.Owin.StaticFiles~~ → **Remove** (use ASP.NET Core static files)

**Microsoft.Extensions Updates:**
- Microsoft.Extensions.Configuration.EnvironmentVariables → **10.0.5**
- Microsoft.Extensions.Configuration.Json → **10.0.5**
- Microsoft.Extensions.DependencyInjection → **10.0.5**
- Microsoft.Extensions.Options.ConfigurationExtensions → **10.0.5**
- System.Diagnostics.DiagnosticSource → **10.0.5**

**Authentication Modernization:**
- ~~Microsoft.IdentityModel.Clients.ActiveDirectory~~ → **Remove**
- **Add:** Microsoft.Identity.Client (latest stable - MSAL)
- **Keep:** System.IdentityModel.Tokens.Jwt (updated to latest compatible with .NET 10)

**Application Insights Updates:**
- ~~Microsoft.ApplicationInsights.TraceListener~~ → **Remove**
- ~~Microsoft.ApplicationInsights.WorkerService~~ → **Update to 2.22.0+** or remove if not needed

**Graph/Bot Framework:**
- Microsoft.Graph 4.16.0 (no change)
- Microsoft.Graph.Communications.Calls 1.2.0.3742 (no change)
- Microsoft.Graph.Communications.Calls.Media → **1.2.0.15690**
- Microsoft.Skype.Bots.Media → **Latest beta compatible with .NET 10** (user accepts beta versions)

**Utilities:**
- Newtonsoft.Json → **13.0.4**
- ~~Microsoft.NETCore.Platforms~~ → **Remove**
- DotNetEnv, NAudio, Newtonsoft.Json.Bson, SharpZipLib (no changes)

---

#### Migration Steps

##### 1. Prerequisites
- ✅ Bot.Model must be upgraded first (dependency)
- ✅ .NET 10.0 SDK installed
- ✅ Familiarity with ASP.NET Core patterns

##### 2. Update Project File

**File:** `Bot.Services\Bot.Services.csproj`

**Change TargetFramework property:**
```xml
<PropertyGroup>
  <OutputType>Library</OutputType>
  <TargetFramework>net10.0</TargetFramework>
  <!-- Remove net461 conditional references -->
</PropertyGroup>
```

**Remove conditional .NET Framework references:**
```xml
<!-- REMOVE this entire ItemGroup: -->
<ItemGroup Condition=" '$(TargetFramework)' == 'net461' ">
  <Reference Include="System" />
  <Reference Include="System.Data" />
</ItemGroup>
```

**Remove legacy assembly references:**
```xml
<!-- REMOVE these references (included in .NET 10 or obsolete): -->
<Reference Include="System.Data.Entity" />
<Reference Include="System.Runtime.Serialization" />
```

**Handle Microsoft.Skype.Internal.Media.H264 reference:**
```xml
<!-- This is a native DLL reference - verify compatibility with .NET 10 -->
<Reference Include="Microsoft.Skype.Internal.Media.H264">
  <HintPath>..\packages\microsoft.skype.bots.media\1.19.0.25-alpha\src\skype_media_lib\Microsoft.Skype.Internal.Media.H264.dll</HintPath>
</Reference>
<!-- May need to update path if Skype.Bots.Media package version changes -->
```

##### 3. Update Package References

**Remove ASP.NET Framework packages:**
```xml
<!-- REMOVE: -->
<PackageReference Include="Microsoft.AspNet.WebApi" Version="5.2.7" />
<PackageReference Include="Microsoft.AspNet.WebApi.Owin" Version="5.2.7" />
```

**Remove OWIN packages:**
```xml
<!-- REMOVE: -->
<PackageReference Include="Microsoft.Owin.Cors" Version="4.2.0" />
<PackageReference Include="Microsoft.Owin.Host.HttpListener" Version="4.2.0" />
<PackageReference Include="Microsoft.Owin.Hosting" Version="4.2.0" />
<PackageReference Include="Microsoft.Owin.StaticFiles" Version="4.2.0" />
```

**Add ASP.NET Core framework reference (if not implicit):**
```xml
<!-- ASP.NET Core apps typically include this automatically, but verify: -->
<FrameworkReference Include="Microsoft.AspNetCore.App" />
```

**Update Microsoft.Extensions packages:**
```xml
<!-- UPDATE all from 6.0.0 to 10.0.5: -->
<PackageReference Include="Microsoft.Extensions.Configuration.EnvironmentVariables" Version="10.0.5" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="10.0.5" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="10.0.5" />
<PackageReference Include="Microsoft.Extensions.Options.ConfigurationExtensions" Version="10.0.5" />
<PackageReference Include="System.Diagnostics.DiagnosticSource" Version="10.0.5" />
```

**Replace authentication package:**
```xml
<!-- REMOVE deprecated: -->
<PackageReference Include="Microsoft.IdentityModel.Clients.ActiveDirectory" Version="5.2.9" />

<!-- ADD modern MSAL: -->
<PackageReference Include="Microsoft.Identity.Client" Version="4.67.0" />
<!-- Or latest stable version -->
```

**Update Application Insights (or remove if not used):**
```xml
<!-- Option 1: Remove if not using Application Insights -->
<!-- REMOVE: -->
<PackageReference Include="Microsoft.ApplicationInsights.TraceListener" Version="2.20.0" />
<PackageReference Include="Microsoft.ApplicationInsights.WorkerService" Version="2.20.0" />

<!-- Option 2: Update to modern Application Insights -->
<PackageReference Include="Microsoft.ApplicationInsights.AspNetCore" Version="2.22.0" />
```

**Update Graph/Bot packages:**
```xml
<!-- UPDATE: -->
<PackageReference Include="Microsoft.Graph.Communications.Calls.Media" Version="1.2.0.15690" />
<PackageReference Include="Microsoft.Skype.Bots.Media" Version="[latest-beta-for-net10]" />
<!-- User accepts beta releases - check NuGet for .NET 10 compatible version -->

<!-- NO CHANGE: -->
<PackageReference Include="Microsoft.Graph" Version="4.16.0" />
<PackageReference Include="Microsoft.Graph.Communications.Calls" Version="1.2.0.3742" />
```

**Update utilities:**
```xml
<!-- UPDATE: -->
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />

<!-- REMOVE: -->
<PackageReference Include="Microsoft.NETCore.Platforms" Version="6.0.1" />

<!-- NO CHANGE: -->
<PackageReference Include="DotNetEnv" Version="2.3.0" />
<PackageReference Include="NAudio" Version="2.0.1" />
<PackageReference Include="Newtonsoft.Json.Bson" Version="1.0.2" />
<PackageReference Include="SharpZipLib" Version="1.3.3" />
```

##### 4. Expected Breaking Changes

**CATEGORY 1: ASP.NET Framework → ASP.NET Core Migration (76 issues)**

**Issue:** `System.Web.Http.ApiController` class not available in ASP.NET Core

**Files Likely Affected:** API controller classes (15 files with incidents)

**Migration Pattern:**

**OLD (ASP.NET Web API):**
```csharp
using System.Web.Http;

[RoutePrefix("api/calling")]
public class CallingController : ApiController
{
    [HttpPost]
    [Route("call")]
    public HttpResponseMessage HandleCall([FromBody] CallRequest request)
    {
        // Process call
        return Request.CreateResponse(HttpStatusCode.OK, response);
    }
}
```

**NEW (ASP.NET Core):**
```csharp
using Microsoft.AspNetCore.Mvc;

[Route("api/calling")]
[ApiController]
public class CallingController : ControllerBase
{
    [HttpPost("call")]
    public ActionResult<CallResponse> HandleCall([FromBody] CallRequest request)
    {
        // Process call
        return Ok(response);
    }
}
```

**Key Changes:**
- `ApiController` → `ControllerBase`
- `HttpResponseMessage` → `ActionResult<T>` or `IActionResult`
- `Request.CreateResponse()` → `Ok()`, `BadRequest()`, `NotFound()`, etc.
- Routing: `[RoutePrefix]` → `[Route]`, attribute routes combined
- `[FromBody]` still works but often implicit in ASP.NET Core

**CATEGORY 2: OWIN Middleware → ASP.NET Core Middleware**

**Issue:** OWIN packages removed, need ASP.NET Core equivalents

**OLD (OWIN Startup):**
```csharp
using Owin;
using Microsoft.Owin.Cors;

public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        var config = new HttpConfiguration();
        config.MapHttpAttributeRoutes();

        app.UseCors(CorsOptions.AllowAll);
        app.UseStaticFiles("/files");
        app.UseWebApi(config);
    }
}
```

**NEW (ASP.NET Core Startup/Program.cs):**
```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin());
});

var app = builder.Build();

// Configure middleware pipeline
app.UseCors();
app.UseStaticFiles();
app.MapControllers();

app.Run();
```

**Key Changes:**
- `IAppBuilder` → `WebApplication` / `IApplicationBuilder`
- `HttpConfiguration` → Service registration in `IServiceCollection`
- OWIN middleware → ASP.NET Core middleware equivalents
- May need to create `Program.cs` if using Startup.cs pattern

**CATEGORY 3: JWT Authentication Modernization**

**Issue:** `System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler.ValidateToken` API changed

**OLD:**
```csharp
using System.IdentityModel.Tokens.Jwt;

var handler = new JwtSecurityTokenHandler();
SecurityToken validatedToken;
var principal = handler.ValidateToken(token, validationParameters, out validatedToken);
```

**NEW:**
```csharp
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

var handler = new JwtSecurityTokenHandler();
var result = handler.ValidateToken(token, validationParameters);
// result.SecurityToken, result.ClaimsIdentity available
```

**Key Changes:**
- `out SecurityToken` parameter removed
- Returns `TokenValidationResult` object
- Access validated token via result properties

**CATEGORY 4: Active Directory Authentication Library (ADAL) → MSAL**

**Issue:** Microsoft.IdentityModel.Clients.ActiveDirectory deprecated

**OLD (ADAL):**
```csharp
using Microsoft.IdentityModel.Clients.ActiveDirectory;

var context = new AuthenticationContext(authority);
var result = await context.AcquireTokenAsync(resource, clientCredential);
var token = result.AccessToken;
```

**NEW (MSAL):**
```csharp
using Microsoft.Identity.Client;

var app = ConfidentialClientApplicationBuilder
    .Create(clientId)
    .WithClientSecret(clientSecret)
    .WithAuthority(authority)
    .Build();

var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
var token = result.AccessToken;
```

**Key Changes:**
- `AuthenticationContext` → `IConfidentialClientApplication`
- `AcquireTokenAsync` → `AcquireTokenForClient().ExecuteAsync()`
- API redesigned around builder pattern

**CATEGORY 5: HttpRequestMessage.Properties → Options**

**Issue:** `HttpRequestMessage.Properties` changed to strongly-typed options

**OLD:**
```csharp
request.Properties["key"] = value;
var value = request.Properties["key"];
```

**NEW:**
```csharp
request.Options.Set(new HttpRequestOptionsKey<T>("key"), value);
var value = request.Options.TryGetValue(new HttpRequestOptionsKey<T>("key"), out var val) ? val : default;
```

**CATEGORY 6: System.Uri and HttpContent Behavioral Changes (36 issues)**

**Issue:** Minor behavioral differences in URL handling and HTTP content processing

**Mitigation:**
- Review URL construction code (Uri constructors)
- Test HTTP client calls thoroughly
- Verify media streaming with NAudio integration
- Check for any assumptions about URI normalization

##### 5. Code Modifications

**Step-by-Step Code Changes:**

**5.1. Update Controller Base Classes**

**Files:** All controller classes (likely in `Controllers/` folder)

Search for: `public class * : ApiController`
Replace with: `public class * : ControllerBase`

Add: `using Microsoft.AspNetCore.Mvc;`
Remove: `using System.Web.Http;`

**5.2. Update Routing Attributes**

Replace routing attributes:
- `[RoutePrefix("path")]` → Move to `[Route("path")]` on controller
- `[Route("subpath")]` → `[HttpGet("subpath")]`, `[HttpPost("subpath")]`, etc.

**5.3. Update Response Patterns**

Replace response creation:
- `Request.CreateResponse(HttpStatusCode.OK, data)` → `Ok(data)`
- `Request.CreateResponse(HttpStatusCode.BadRequest)` → `BadRequest()`
- `Request.CreateResponse(HttpStatusCode.NotFound)` → `NotFound()`

Change return types:
- `HttpResponseMessage` → `ActionResult<T>` or `IActionResult`

**5.4. Update Startup/Configuration**

**If using Startup.cs:**
- Create `Program.cs` using minimal hosting model (see CATEGORY 2 example)
- Or update `Startup.cs` to ASP.NET Core pattern

**If using OWIN in separate class:**
- Migrate to ASP.NET Core middleware pipeline
- Update service registration to use `IServiceCollection`

**5.5. Update JWT Authentication**

**File:** Authentication handler/middleware

- Update `JwtSecurityTokenHandler.ValidateToken` calls
- Handle `TokenValidationResult` instead of `out` parameter
- Test token validation end-to-end

**5.6. Replace ADAL with MSAL**

**Files:** Authentication/Graph API client code

- Replace `AuthenticationContext` with `ConfidentialClientApplicationBuilder`
- Update token acquisition flows
- Update Graph API client initialization
- Test authentication flow

**5.7. Update HttpRequestMessage Usage**

**Files:** HTTP client code, middleware

- Replace `.Properties` dictionary with `.Options`
- Use strongly-typed options keys
- Test request/response handling

**5.8. Review Native DLL References**

**File:** `Bot.Services.csproj`

- Verify `Microsoft.Skype.Internal.Media.H264.dll` path still valid
- Update path if Microsoft.Skype.Bots.Media package version changed
- Test media handling functionality

##### 6. Testing Strategy

**6.1. Unit Tests**
- Test individual controllers and services
- Verify dependency injection works
- Test authentication logic
- Validate data models

**6.2. Integration Tests**
- **Critical:** Test all API endpoints
  - Verify routing works correctly
  - Test request/response serialization
  - Validate authentication/authorization
- Test Graph API integration
- Test Bot Framework media handling
- Test CORS functionality
- Test static file serving

**6.3. Authentication Tests**
- Verify JWT token validation
- Test MSAL token acquisition
- Validate Graph API authentication
- Test authenticated endpoints

**6.4. Bot Framework Tests**
- Test Skype Bot Media integration
- Verify audio/video handling (NAudio)
- Test call handling
- Validate media streaming

**6.5. Performance Tests**
- Compare API response times (baseline vs upgraded)
- Monitor memory usage
- Test under load

##### 7. Validation Checklist

**Project Configuration:**
- [ ] Project file TargetFramework updated to `net10.0`
- [ ] All ASP.NET Framework packages removed
- [ ] All OWIN packages removed
- [ ] ASP.NET Core framework reference added (if needed)
- [ ] All Microsoft.Extensions packages updated to 10.0.5
- [ ] ADAL package removed, MSAL package added
- [ ] Application Insights packages updated or removed
- [ ] Graph/Bot packages updated
- [ ] Newtonsoft.Json updated to 13.0.4
- [ ] Microsoft.NETCore.Platforms removed

**Code Changes:**
- [ ] All controllers inherit from `ControllerBase`
- [ ] All routing attributes updated
- [ ] All response patterns updated (no `Request.CreateResponse`)
- [ ] Startup/Program.cs migrated to ASP.NET Core
- [ ] CORS middleware migrated
- [ ] Static files middleware migrated
- [ ] JWT authentication updated
- [ ] ADAL authentication replaced with MSAL
- [ ] HttpRequestMessage.Properties updated to Options

**Build & Compilation:**
- [ ] Project builds without errors: `dotnet build Bot.Services\Bot.Services.csproj`
- [ ] No warnings about deprecated APIs
- [ ] No package dependency conflicts
- [ ] Bot.Model reference resolves correctly

**Testing:**
- [ ] All unit tests pass
- [ ] All API endpoints respond correctly
- [ ] Authentication works (JWT validation)
- [ ] Graph API integration works
- [ ] Bot Framework media handling works
- [ ] CORS works correctly
- [ ] Static files serve correctly
- [ ] No runtime exceptions

**Security:**
- [ ] No deprecated authentication packages remain
- [ ] JWT validation secure
- [ ] Graph API permissions correct
- [ ] No security vulnerabilities in packages

**Performance:**
- [ ] API response times acceptable
- [ ] Memory usage reasonable
- [ ] No performance regressions

---

---

## Package Update Reference

This section consolidates all package updates across the three projects for the All-At-Once upgrade.

### Package Update Summary

| Status | Count | Percentage |
|--------|-------|------------|
| 🔄 Upgrade Recommended | 7 | 25.9% |
| ⚠️ Incompatible (Replace/Remove) | 10 | 37.0% |
| 🗑️ Remove (Framework-included) | 2 | 7.4% |
| ✅ Compatible (No Change) | 8 | 29.6% |
| **Total Packages** | **27** | **100%** |

---

### Package Updates by Category

#### Microsoft.Extensions Packages (Upgrade Recommended)

**Scope:** 5 packages in Bot.Services + 1 in Bot.Console

| Package | Current | Target | Projects | Priority |
|---------|---------|--------|----------|----------|
| Microsoft.Extensions.Configuration.EnvironmentVariables | 6.0.0 | **10.0.5** | Bot.Services | High |
| Microsoft.Extensions.Configuration.Json | 6.0.0 | **10.0.5** | Bot.Services | High |
| Microsoft.Extensions.DependencyInjection | 6.0.0 | **10.0.5** | Bot.Services | High |
| Microsoft.Extensions.Options.ConfigurationExtensions | 6.0.0 | **10.0.5** | Bot.Services | High |
| Microsoft.Extensions.FileProviders.Abstractions | 6.0.0 | **10.0.5** | Bot.Console | Medium |

**Reason:** Align with .NET 10 versions for compatibility and security updates

---

#### Diagnostics & Telemetry (Upgrade/Update)

| Package | Current | Target | Projects | Priority |
|---------|---------|--------|----------|----------|
| System.Diagnostics.DiagnosticSource | 6.0.0 | **10.0.5** | Bot.Services | High |
| Microsoft.ApplicationInsights.TraceListener | 2.20.0 | **Remove or Update** | Bot.Services | Medium (Deprecated) |
| Microsoft.ApplicationInsights.WorkerService | 2.20.0 | **Remove or Update** | Bot.Services | Medium (Deprecated) |

**Options for Application Insights:**
- **Option 1 (Recommended):** Remove if not actively using telemetry
- **Option 2:** Update to `Microsoft.ApplicationInsights.AspNetCore` 2.22.0+
- **Option 3:** Keep existing if compatible (not recommended - deprecated)

---

#### JSON Serialization (Security Update)

| Package | Current | Target | Projects | Priority |
|---------|---------|--------|----------|----------|
| Newtonsoft.Json | 13.0.1 | **13.0.4** | Bot.Model, Bot.Services | **Critical (Security)** |

**Reason:** Security patches included in 13.0.4

---

#### ASP.NET Framework (Incompatible - Remove)

**Scope:** Bot.Services only

| Package | Current | Action | Replacement |
|---------|---------|--------|-------------|
| Microsoft.AspNet.WebApi | 5.2.7 | **Remove** | ASP.NET Core (FrameworkReference) |
| Microsoft.AspNet.WebApi.Owin | 5.2.7 | **Remove** | ASP.NET Core (FrameworkReference) |

**Migration Path:** Replace with ASP.NET Core framework reference + code migration (see Breaking Changes Catalog)

---

#### OWIN Packages (Incompatible - Remove)

**Scope:** Bot.Services (4 packages), Bot.Model (1 package)

| Package | Current | Action | Replacement |
|---------|---------|--------|-------------|
| Microsoft.Owin.Cors | 4.2.0 | **Remove** | ASP.NET Core CORS middleware (built-in) |
| Microsoft.Owin.Host.HttpListener | 4.2.0 | **Remove** | Kestrel web server (built-in) |
| Microsoft.Owin.Hosting | 4.2.0 | **Remove** | ASP.NET Core hosting (built-in) |
| Microsoft.Owin.StaticFiles | 4.2.0 | **Remove** | ASP.NET Core static files middleware (built-in) |

**Migration Path:** Replace with equivalent ASP.NET Core middleware

---

#### Authentication & Identity (Deprecated - Replace)

**Scope:** Bot.Services only

| Package | Current | Action | Replacement | Priority |
|---------|---------|--------|-------------|----------|
| Microsoft.IdentityModel.Clients.ActiveDirectory (ADAL) | 5.2.9 | **Remove** | **Microsoft.Identity.Client (MSAL)** latest stable | **High (End of Support)** |

**Migration Notes:**
- ADAL is deprecated and no longer supported
- MSAL (Microsoft Authentication Library) is the modern replacement
- API patterns differ significantly - code changes required
- MSAL recommended version: 4.67.0 or latest stable

---

#### Graph & Bot Framework (Update)

**Scope:** Bot.Model, Bot.Services

| Package | Current | Target | Projects | Priority | Notes |
|---------|---------|--------|----------|----------|-------|
| Microsoft.Graph | 4.16.0 | **No Change** | Bot.Model, Bot.Services | - | Compatible |
| Microsoft.Graph.Communications.Calls | 1.2.0.3742 | **No Change** | Bot.Model, Bot.Services | - | Compatible |
| Microsoft.Graph.Communications.Calls.Media | 1.2.0.3742 | **1.2.0.15690** | Bot.Services | Medium | Updated version available |
| Microsoft.Skype.Bots.Media | 1.21.0.241-alpha | **Latest .NET 10 Beta** | Bot.Services | High | **User accepts beta releases** |

**Microsoft.Skype.Bots.Media Notes:**
- Current version is alpha/beta
- Need .NET 10 compatible beta version
- Check NuGet.org for latest prerelease: `dotnet nuget list Microsoft.Skype.Bots.Media --prerelease`
- User explicitly accepts beta releases for this package

---

#### Framework-Included Packages (Remove)

**Scope:** Bot.Console (2 packages), Bot.Model (1 package), Bot.Services (1 package)

| Package | Current | Action | Projects | Reason |
|---------|---------|--------|----------|--------|
| System.Data.DataSetExtensions | 4.5.0 | **Remove** | Bot.Console, Bot.Model | Included in .NET 10 framework |
| Microsoft.NETCore.Platforms | 6.0.1 | **Remove** | Bot.Services | Included in .NET 10 SDK |

**Migration Path:** Simply remove - functionality built into framework

---

#### Compatible Packages (No Change)

**Scope:** Various projects

| Package | Version | Projects | Status |
|---------|---------|----------|--------|
| DotNetEnv | 2.3.0 | Bot.Services | ✅ Compatible |
| Microsoft.CSharp | 4.7.0 | Bot.Console, Bot.Model | ✅ Compatible |
| NAudio | 2.0.1 | Bot.Services | ✅ Compatible |
| Newtonsoft.Json.Bson | 1.0.2 | Bot.Services | ✅ Compatible |
| SharpZipLib | 1.3.3 | Bot.Services | ✅ Compatible |

**No action needed** - these packages are compatible with .NET 10

---

### Package Update Execution Order

For All-At-Once strategy, all packages update simultaneously. However, understanding logical dependencies helps with troubleshooting:

**1. Framework-Included Packages (Remove First)**
- Remove System.Data.DataSetExtensions
- Remove Microsoft.NETCore.Platforms

**2. Core Framework Updates (Update Next)**
- Update all Microsoft.Extensions.* packages to 10.0.5
- Update System.Diagnostics.DiagnosticSource to 10.0.5

**3. Remove Incompatible Packages**
- Remove all Microsoft.AspNet.* packages
- Remove all Microsoft.Owin.* packages
- Remove Microsoft.IdentityModel.Clients.ActiveDirectory

**4. Add Replacement Packages**
- Add Microsoft.Identity.Client (MSAL)
- Add ASP.NET Core framework reference if needed
- Update/add Application Insights packages if keeping telemetry

**5. Update Remaining Packages**
- Update Newtonsoft.Json to 13.0.4 (security)
- Update Microsoft.Graph.Communications.Calls.Media to 1.2.0.15690
- Update Microsoft.Skype.Bots.Media to latest .NET 10 beta

**6. Verify Compatible Packages**
- Ensure DotNetEnv, NAudio, SharpZipLib, etc. remain unchanged

---

### Package Update Verification

After all package updates applied:

**Restore Dependencies:**
```bash
dotnet restore RickrollBot.sln
```

**Check for Conflicts:**
```bash
dotnet list package --vulnerable
dotnet list package --deprecated
dotnet list package --outdated
```

**Expected Outcome:**
- ✅ No package conflicts
- ✅ No security vulnerabilities
- ✅ No deprecated packages (except intentionally kept compatible ones)
- ✅ All packages restore successfully

---

## Breaking Changes Catalog

This section catalogs all breaking changes identified in the assessment and provides migration guidance.

### Breaking Change Categories

| Category | Count | Severity | Primary Project |
|----------|-------|----------|-----------------|
| **ASP.NET Framework → Core** | 76 | 🔴 High | Bot.Services |
| **JWT Authentication** | 3 | 🟡 Medium | Bot.Services |
| **Behavioral Changes** | 36 | 🟢 Low | Bot.Services |
| **Package Replacements** | 10 | 🟡 Medium | Bot.Services, Bot.Model |

---

### CATEGORY 1: ASP.NET Framework to ASP.NET Core (76 Breaking Changes)

**Affected Files:** 15 files in Bot.Services with incidents

#### Breaking Change 1.1: ApiController Class

**Issue:** `System.Web.Http.ApiController` does not exist in ASP.NET Core

**Occurrences:** Estimated 12 controller classes

**Migration:**

| ASP.NET Web API (.NET Framework) | ASP.NET Core |
|----------------------------------|--------------|
| `using System.Web.Http;` | `using Microsoft.AspNetCore.Mvc;` |
| `public class MyController : ApiController` | `public class MyController : ControllerBase` |
| `HttpResponseMessage` | `ActionResult<T>` or `IActionResult` |

**Code Example:**

**Before:**
```csharp
using System.Web.Http;

public class CallController : ApiController
{
    public HttpResponseMessage Get()
    {
        var data = GetData();
        return Request.CreateResponse(HttpStatusCode.OK, data);
    }
}
```

**After:**
```csharp
using Microsoft.AspNetCore.Mvc;

[ApiController]
public class CallController : ControllerBase
{
    public ActionResult<DataModel> Get()
    {
        var data = GetData();
        return Ok(data);
    }
}
```

---

#### Breaking Change 1.2: Routing Attributes

**Issue:** Routing attributes have different syntax and behavior

**Occurrences:** Estimated 18 route attributes + 6 HTTP method attributes

**Migration:**

| ASP.NET Web API | ASP.NET Core |
|-----------------|--------------|
| `[RoutePrefix("api/controller")]` | `[Route("api/controller")]` (on controller) |
| `[Route("action")]` (on method) | `[HttpGet("action")]` / `[HttpPost("action")]` |
| `[HttpPost]` + `[Route("action")]` | `[HttpPost("action")]` (combined) |

**Code Example:**

**Before:**
```csharp
[RoutePrefix("api/calling")]
public class CallingController : ApiController
{
    [HttpGet]
    [Route("status/{id}")]
    public HttpResponseMessage GetStatus(string id)
    {
        // ...
    }

    [HttpPost]
    [Route("start")]
    public HttpResponseMessage StartCall([FromBody] CallRequest request)
    {
        // ...
    }
}
```

**After:**
```csharp
[Route("api/calling")]
[ApiController]
public class CallingController : ControllerBase
{
    [HttpGet("status/{id}")]
    public ActionResult<CallStatus> GetStatus(string id)
    {
        // ...
    }

    [HttpPost("start")]
    public ActionResult<CallResponse> StartCall([FromBody] CallRequest request)
    {
        // ...
    }
}
```

---

#### Breaking Change 1.3: HttpRequestMessage Extension Methods

**Issue:** `System.Net.Http.HttpRequestMessageExtensions.CreateResponse` does not exist

**Occurrences:** Estimated 10 instances

**Migration:**

| ASP.NET Web API | ASP.NET Core |
|-----------------|--------------|
| `Request.CreateResponse(HttpStatusCode.OK, data)` | `Ok(data)` |
| `Request.CreateResponse(HttpStatusCode.BadRequest, error)` | `BadRequest(error)` |
| `Request.CreateResponse(HttpStatusCode.NotFound)` | `NotFound()` |
| `Request.CreateResponse(HttpStatusCode.Created, data)` | `CreatedAtAction("ActionName", data)` |

**Code Example:**

**Before:**
```csharp
public HttpResponseMessage ProcessRequest(RequestModel model)
{
    if (model == null)
        return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid request");

    var result = ProcessData(model);

    if (result == null)
        return Request.CreateResponse(HttpStatusCode.NotFound);

    return Request.CreateResponse(HttpStatusCode.OK, result);
}
```

**After:**
```csharp
public ActionResult<ResultModel> ProcessRequest(RequestModel model)
{
    if (model == null)
        return BadRequest("Invalid request");

    var result = ProcessData(model);

    if (result == null)
        return NotFound();

    return Ok(result);
}
```

---

#### Breaking Change 1.4: HttpConfiguration and OWIN Integration

**Issue:** `System.Web.Http.HttpConfiguration` and OWIN middleware configuration not available

**Occurrences:** Estimated 1 Startup class

**Migration:**

**Before (OWIN):**
```csharp
using Owin;
using System.Web.Http;
using Microsoft.Owin.Cors;

public class Startup
{
    public void Configuration(IAppBuilder app)
    {
        var config = new HttpConfiguration();
        config.MapHttpAttributeRoutes();
        config.MessageHandlers.Add(new CustomHandler());
        config.Services.Add(typeof(IExceptionLogger), new CustomLogger());

        app.UseCors(CorsOptions.AllowAll);
        app.UseStaticFiles("/files");
        app.UseWebApi(config);
    }
}
```

**After (ASP.NET Core):**
```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure services (replaces HttpConfiguration)
        builder.Services.AddControllers();
        builder.Services.AddSingleton<IExceptionLogger, CustomLogger>();
        builder.Services.AddTransient<CustomHandler>();

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy => 
                policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });

        var app = builder.Build();

        // Configure middleware pipeline (replaces IAppBuilder)
        app.UseCors();
        app.UseStaticFiles();
        app.MapControllers();

        app.Run();
    }
}
```

---

### CATEGORY 2: JWT Authentication (3 Breaking Changes)

**Affected Files:** Authentication handlers/middleware in Bot.Services

#### Breaking Change 2.1: JwtSecurityTokenHandler.ValidateToken

**Issue:** `ValidateToken` method signature changed - `out SecurityToken` parameter removed

**Occurrences:** Estimated 1-2 instances

**Migration:**

**Before:**
```csharp
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

var handler = new JwtSecurityTokenHandler();
SecurityToken validatedToken;

try
{
    var principal = handler.ValidateToken(
        token, 
        validationParameters, 
        out validatedToken
    );

    // Use principal and validatedToken
}
catch (SecurityTokenException ex)
{
    // Handle validation failure
}
```

**After:**
```csharp
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;

var handler = new JwtSecurityTokenHandler();

try
{
    var result = handler.ValidateToken(token, validationParameters);
    var principal = new ClaimsPrincipal(result.ClaimsIdentity);
    var validatedToken = result.SecurityToken;

    // Use principal and validatedToken
}
catch (SecurityTokenException ex)
{
    // Handle validation failure
}
```

**Key Differences:**
- Returns `TokenValidationResult` object instead of `ClaimsPrincipal`
- No `out` parameter - access token via `result.SecurityToken`
- Access claims via `result.ClaimsIdentity`

---

### CATEGORY 3: Active Directory Authentication Library (ADAL) to MSAL

**Affected Files:** Authentication/Graph API client code in Bot.Services

#### Breaking Change 3.1: ADAL Package Deprecated

**Issue:** `Microsoft.IdentityModel.Clients.ActiveDirectory` is end-of-life and incompatible with .NET 10

**Migration to MSAL:**

**Before (ADAL):**
```csharp
using Microsoft.IdentityModel.Clients.ActiveDirectory;

var authority = "https://login.microsoftonline.com/{tenantId}";
var resource = "https://graph.microsoft.com";
var context = new AuthenticationContext(authority);
var credential = new ClientCredential(clientId, clientSecret);

var result = await context.AcquireTokenAsync(resource, credential);
var accessToken = result.AccessToken;

// Use with Graph client
var graphClient = new GraphServiceClient(
    new DelegateAuthenticationProvider(req =>
    {
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return Task.CompletedTask;
    })
);
```

**After (MSAL):**
```csharp
using Microsoft.Identity.Client;
using Microsoft.Graph;
using Azure.Identity;

var options = new ClientSecretCredentialOptions
{
    AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
};

var clientSecretCredential = new ClientSecretCredential(
    tenantId, clientId, clientSecret, options);

// Modern approach: Use Azure.Identity with Graph SDK
var graphClient = new GraphServiceClient(clientSecretCredential);

// Or use MSAL directly:
var app = ConfidentialClientApplicationBuilder
    .Create(clientId)
    .WithClientSecret(clientSecret)
    .WithAuthority(new Uri($"https://login.microsoftonline.com/{tenantId}"))
    .Build();

var scopes = new[] { "https://graph.microsoft.com/.default" };
var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();
var accessToken = result.AccessToken;
```

**Key Differences:**
- `AuthenticationContext` → `IConfidentialClientApplication` or `ClientSecretCredential`
- `AcquireTokenAsync` → `AcquireTokenForClient().ExecuteAsync()`
- Resource-based scopes → Scope-based authentication
- Recommendation: Use `Azure.Identity` package for simplest migration with Graph SDK

---

### CATEGORY 4: Behavioral Changes (36 Issues)

#### Breaking Change 4.1: System.Uri Behavioral Changes

**Issue:** URI parsing and normalization behavior differs in .NET 10

**Occurrences:** 20 instances

**Impact:** Low - runtime behavioral differences

**Guidance:**
- Review code constructing URIs dynamically
- Test URL construction thoroughly
- Pay attention to:
  - Relative URI resolution
  - URI escaping/unescaping
  - AbsoluteUri property values
  - Query string handling

**Testing Focus:**
```csharp
// Test these patterns:
var baseUri = new Uri("https://example.com/api/");
var relativeUri = new Uri(baseUri, "endpoint");
// Verify: relativeUri.AbsoluteUri

// Test query strings
var uriWithQuery = new Uri("https://example.com/api?param=value");
// Verify query string parsing
```

---

#### Breaking Change 4.2: HttpContent Behavioral Changes

**Issue:** `System.Net.Http.HttpContent` handling differences in .NET 10

**Occurrences:** 13 instances

**Impact:** Low - runtime behavioral differences

**Guidance:**
- Review HTTP client usage
- Test content serialization/deserialization
- Verify media streaming (especially with NAudio integration)
- Pay attention to:
  - Content-Type headers
  - Stream disposal
  - Buffer sizes
  - Async reading patterns

**Testing Focus:**
```csharp
// Test HTTP content handling:
var content = new StringContent(json, Encoding.UTF8, "application/json");
var response = await httpClient.PostAsync(url, content);
var result = await response.Content.ReadAsStringAsync();

// Test streaming content (media scenarios):
var streamContent = new StreamContent(audioStream);
streamContent.Headers.ContentType = new MediaTypeHeaderValue("audio/wav");
```

---

#### Breaking Change 4.3: TimeSpan.FromMinutes Source Incompatibility

**Issue:** `TimeSpan.FromMinutes(Double)` may have source-level changes

**Occurrences:** 2 instances

**Impact:** Low - likely just recompilation needed

**Guidance:**
- Code should work unchanged
- Compiler may issue warnings
- Verify timeout values haven't changed

---

### CATEGORY 5: Package Removal Breaking Changes

#### Breaking Change 5.1: System.Data.Entity Removal

**Issue:** `System.Data.Entity` reference removed (legacy Entity Framework)

**Occurrences:** 1 reference in Bot.Services.csproj

**Impact:** Depends on usage

**Guidance:**
- If not actually using Entity Framework 6.x, simply remove reference
- If using EF 6.x:
  - **Option 1:** Migrate to Entity Framework Core (significant effort)
  - **Option 2:** Add `EntityFramework` NuGet package explicitly (not recommended for .NET 10)

**Check for Usage:**
```csharp
// Search for:
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

// If found, EF migration needed
```

---

#### Breaking Change 5.2: System.Runtime.Serialization Removal

**Issue:** `System.Runtime.Serialization` assembly reference removed

**Occurrences:** 1 reference in Bot.Services.csproj

**Impact:** Depends on usage

**Guidance:**
- Serialization types like `DataContract`, `DataMember` now in `System.Runtime.Serialization.Primitives` NuGet package
- Check if code uses:
  - `[DataContract]` / `[DataMember]` attributes
  - `DataContractSerializer`
  - `NetDataContractSerializer`

**If Needed:**
```xml
<PackageReference Include="System.Runtime.Serialization.Primitives" Version="4.3.0" />
<!-- Or migrate to modern JSON serialization with Newtonsoft.Json or System.Text.Json -->
```

---

### Breaking Changes Summary & Priority

| Priority | Category | Count | Effort | Risk |
|----------|----------|-------|--------|------|
| 🔴 **Critical** | ASP.NET Framework → Core | 76 | High | High |
| 🟡 **High** | ADAL → MSAL | 3 | Medium | Medium |
| 🟡 **High** | JWT Authentication | 3 | Low-Medium | Medium |
| 🟢 **Medium** | Package Removals | 10 | Low-Medium | Low-Medium |
| 🟢 **Low** | Behavioral Changes | 36 | Low (Testing) | Low |

**Execution Strategy:**
1. Address Critical (ASP.NET Core migration) first - blocks compilation
2. Address High priority (ADAL, JWT) next - affects functionality
3. Handle Medium priority (package removals) - may block compilation
4. Validate Low priority (behavioral changes) through testing

---

---

## Risk Management

### High-Level Risk Assessment

| Risk Level | Projects | Count | Total LOC | Description |
|------------|----------|-------|-----------|-------------|
| 🟢 **Low** | Bot.Model, Bot.Console | 2 | 789 | Minimal API issues, straightforward package updates |
| 🟡 **Medium** | Bot.Services | 1 | 5,390 | ASP.NET Framework → Core migration, 141 API issues |
| 🔴 **High** | None | 0 | 0 | No high-risk projects identified |

**Overall Solution Risk: 🟡 Medium**

The upgrade risk is concentrated in Bot.Services due to ASP.NET Framework dependencies. However, this is a well-documented migration path with clear patterns.

---

### High-Risk Changes

| Project | Risk Level | Description | Mitigation Strategy |
|---------|------------|-------------|---------------------|
| Bot.Services | 🟡 Medium | **ASP.NET Framework → ASP.NET Core Migration**<br/>- 76 System.Web.Http incompatibilities<br/>- ApiController → ControllerBase<br/>- Routing attribute changes<br/>- HttpRequestMessage pattern changes | - Follow official ASP.NET Core migration guide<br/>- Use System.Web.Adapters as temporary bridge if needed<br/>- Comprehensive API endpoint testing<br/>- Validate all routes and bindings |
| Bot.Services | 🟡 Medium | **OWIN Middleware Incompatibilities**<br/>- 5 incompatible OWIN packages<br/>- Microsoft.Owin.* → ASP.NET Core equivalents<br/>- Middleware configuration patterns differ | - Replace with ASP.NET Core middleware<br/>- Update Startup.cs configuration<br/>- Test CORS and static files serving<br/>- Validate authentication pipeline |
| Bot.Services | 🟡 Medium | **JWT Authentication Modernization**<br/>- System.IdentityModel.Tokens.Jwt API changes<br/>- Token validation parameter changes<br/>- SecurityToken output parameter modifications | - Update to Microsoft.IdentityModel.Tokens 8.x<br/>- Review token validation code<br/>- Test authentication end-to-end<br/>- Verify Graph API integration |
| Bot.Services | 🟢 Low | **Behavioral Changes in System.Uri and HttpContent**<br/>- 36 behavioral change warnings<br/>- May affect URL handling and HTTP operations | - Review URL construction logic<br/>- Test HTTP client calls<br/>- Validate media streaming (NAudio integration) |

---

### Security Vulnerabilities

**Deprecated Packages Requiring Replacement:**

| Package | Current Version | Issue | Remediation | Severity |
|---------|-----------------|-------|-------------|----------|
| **Microsoft.IdentityModel.Clients.ActiveDirectory** | 5.2.9 | ⚠️ Deprecated - End of support<br/>No longer maintained by Microsoft | Replace with **Microsoft.Identity.Client (MSAL)** latest version<br/>Update authentication flows | 🟡 Medium |
| **Microsoft.ApplicationInsights.TraceListener** | 2.20.0 | ⚠️ Deprecated package | Migrate to modern Application Insights SDK<br/>Use **Microsoft.ApplicationInsights.AspNetCore** or remove if not needed | 🟢 Low |
| **Microsoft.ApplicationInsights.WorkerService** | 2.20.0 | ⚠️ Deprecated package | Use **Microsoft.ApplicationInsights.WorkerService 2.22.0+** or newer telemetry approach | 🟢 Low |

**Package Update Security Benefits:**

Several packages have security improvements in newer versions:
- **Newtonsoft.Json 13.0.1 → 13.0.4** - Security patches
- **System.Diagnostics.DiagnosticSource 6.0.0 → 10.0.5** - Includes security fixes
- **Microsoft.Extensions.* 6.0.0 → 10.0.5** - Updated to latest security patches

---

### Contingency Plans

#### Scenario 1: ASP.NET Core Migration Blocking Issues

**Problem:** System.Web.Http migration proves too complex or incompatible with existing patterns

**Options:**
1. **System.Web.Adapters Package** (temporary bridge)
   - Install `Microsoft.AspNetCore.SystemWebAdapters` package
   - Provides compatibility layer for System.Web APIs
   - Allows incremental migration
   - **Trade-off:** Not a long-term solution, adds dependency

2. **Rewrite API Controllers** (recommended long-term)
   - Convert ApiController → ControllerBase
   - Update routing to ASP.NET Core patterns
   - Modernize request/response handling
   - **Trade-off:** More work upfront, better long-term outcome

3. **Rollback and Incremental Approach**
   - Revert to net472
   - Migrate Bot.Model and Bot.Console first to netstandard2.1
   - Migrate Bot.Services separately with more time
   - **Trade-off:** Longer timeline, multi-targeting complexity

#### Scenario 2: OWIN Middleware Migration Issues

**Problem:** OWIN-based middleware doesn't have direct ASP.NET Core equivalent

**Options:**
1. **Native ASP.NET Core Middleware**
   - Replace Microsoft.Owin.Cors with built-in CORS
   - Replace Microsoft.Owin.StaticFiles with built-in static files
   - Use ASP.NET Core authentication middleware
   - **Trade-off:** Requires configuration changes

2. **Custom Middleware Wrapper**
   - Create ASP.NET Core middleware wrapping existing logic
   - Maintain similar functionality
   - **Trade-off:** Additional code to maintain

#### Scenario 3: Microsoft.Skype.Bots.Media Compatibility

**Problem:** Beta version of Microsoft.Skype.Bots.Media has issues with .NET 10

**Options:**
1. **Try Earlier Beta Version**
   - Test with Microsoft.Skype.Bots.Media 1.21.0.241-alpha or earlier betas
   - Check release notes for .NET 10 support

2. **Contact Microsoft Support**
   - Engage Bot Framework support
   - Request .NET 10 compatible release timeline

3. **Temporary Fallback to .NET 8**
   - Target net8.0 (LTS) instead of net10.0
   - Still benefits from modern .NET
   - **Trade-off:** Miss .NET 10 features

#### Scenario 4: Performance Degradation

**Problem:** Upgraded solution shows performance regression

**Options:**
1. **Profile and Optimize**
   - Use .NET profiling tools
   - Identify bottlenecks
   - Apply .NET 10-specific optimizations

2. **Configuration Tuning**
   - Adjust GC settings
   - Configure threading pool
   - Optimize HTTP client usage

3. **Incremental Investigation**
   - Isolate performance issue to specific project
   - Test components individually
   - Compare with baseline metrics

---

### Rollback Strategy

**If upgrade fails or introduces critical issues:**

1. **Git Branch Rollback**
   ```bash
   git checkout main
   git branch -D upgrade-to-NET10
   ```

2. **Restore Original State**
   - All projects remain on net472
   - Original package versions intact
   - No breaking changes applied

3. **Analyze Failure**
   - Document blocking issues
   - Gather error logs
   - Identify specific incompatibilities

4. **Plan Alternative Approach**
   - Consider incremental strategy
   - Evaluate intermediate frameworks (net8.0)
   - Break into smaller phases

**Rollback Decision Criteria:**
- ❌ Cannot resolve compilation errors after reasonable effort
- ❌ Critical functionality broken with no clear fix
- ❌ Security vulnerabilities introduced
- ❌ Performance degradation >50%
- ❌ Third-party package (Skype Bot Media) incompatible with no alternatives

---

## Testing & Validation Strategy

[To be filled]

---

## Complexity & Effort Assessment

### Per-Project Complexity

| Project | Complexity | Dependencies | Risk | LOC | Files w/ Issues | Key Factors |
|---------|------------|--------------|------|-----|-----------------|-------------|
| **Bot.Model** | 🟢 Low | 0 projects<br/>3 packages | Low | 673 | 1/14 | - No API incompatibilities<br/>- 3 package updates (straightforward)<br/>- Leaf node (no upstream impact) |
| **Bot.Services** | 🟡 Medium | 1 project<br/>25 packages | Medium | 5,390 | 15/39 | - **141 API issues** (82 binary, 23 source, 36 behavioral)<br/>- ASP.NET Framework → Core migration<br/>- 10 incompatible packages<br/>- OWIN middleware replacement<br/>- JWT authentication updates |
| **Bot.Console** | 🟢 Low | 2 projects<br/>2 packages | Low | 116 | 1/3 | - No API incompatibilities<br/>- 2 package updates<br/>- Entry point (no downstream impact) |

### Phase Complexity Assessment

#### Phase 0: Prerequisites
**Complexity:** 🟢 Low
- Standard environment checks
- SDK verification
- Branch validation

#### Phase 1: Atomic Upgrade
**Complexity:** 🟡 Medium (driven by Bot.Services)

**Breakdown by Operation:**

| Operation | Complexity | Driver |
|-----------|------------|--------|
| Project file updates (TargetFramework) | 🟢 Low | Simple property changes |
| Package reference updates | 🟢 Low-Medium | Straightforward version bumps + some removals |
| ASP.NET Core migration | 🟡 Medium | 76 API incompatibilities requiring code changes |
| JWT authentication modernization | 🟢 Low-Medium | API signature changes, well-documented |
| OWIN middleware replacement | 🟡 Medium | Architecture pattern changes |
| Behavioral change accommodations | 🟢 Low | Primarily testing impact |
| Build and compilation fixes | 🟡 Medium | Iterative error resolution |

**Concentrated Effort:** 87% of complexity in Bot.Services (5,390 LOC, 141 issues)

#### Phase 2: Testing & Validation
**Complexity:** 🟢 Low-Medium
- Unit testing: Low (if tests exist)
- Integration testing: Medium (API endpoints, authentication)
- Smoke testing: Low (application startup)

### Dependency-Ordered Complexity Flow

Following the dependency chain:

```
Bot.Model (🟢 Low)
   ↓ Impact propagates upward
Bot.Services (🟡 Medium) ← Primary effort concentration
   ↓ Impact propagates upward  
Bot.Console (🟢 Low)
```

**Validation Sequence:**
1. ✅ Bot.Model: Quick validation (minimal changes)
2. ✅ Bot.Services: **Critical validation** (most complexity)
3. ✅ Bot.Console: Final integration validation

### Resource Requirements

#### Skills Needed

| Skill Area | Importance | Rationale |
|------------|------------|-----------|
| **.NET Framework → .NET Core migration** | ⚙️⚙️⚙️ High | ASP.NET Web API → ASP.NET Core patterns |
| **ASP.NET Core fundamentals** | ⚙️⚙️⚙️ High | Middleware, routing, controller patterns |
| **Package management & NuGet** | ⚙️⚙️ Medium | 27 packages, version resolution |
| **JWT/OAuth authentication** | ⚙️⚙️ Medium | Token validation API changes |
| **C# language features** | ⚙️ Low-Medium | Modern C# patterns (already using latest) |
| **Bot Framework / Skype SDK** | ⚙️⚙️ Medium | Media handling, Bot.Services integration |

#### Parallel Execution Capacity

**All-At-Once Strategy Considerations:**
- All project files can be edited in parallel (3 developers)
- Breaking changes in Bot.Services require sequential attention
- Testing should follow dependency order (sequential)

**Realistic Approach:**
- **Single developer:** Execute all changes sequentially
- **Small team (2-3):** Split by project (one focuses on Bot.Services)
- **Validation:** Always sequential (bottom-up)

### Effort Distribution

Estimated relative effort by project (not time-based):

| Project | Relative Effort | Percentage | Justification |
|---------|-----------------|------------|---------------|
| Bot.Model | 🟢 Low | ~10% | Simple updates, no API issues |
| Bot.Services | 🟡 Medium-High | ~75% | **Primary complexity driver:** ASP.NET migration, API fixes, package replacements |
| Bot.Console | 🟢 Low | ~5% | Minimal changes |
| Testing/Validation | 🟢 Medium | ~10% | Comprehensive but straightforward |
| **Total** | **Medium** | **100%** | Concentrated in single project |

**Note:** These are relative complexity indicators, not time estimates. Actual duration depends on developer experience, environment setup, and unforeseen issues.

---

## Testing & Validation Strategy

### Testing Approach

The All-At-Once strategy requires comprehensive testing after the atomic upgrade completes. Testing follows the dependency order (bottom-up) to catch issues early.

---

### Phase-by-Phase Testing Requirements

#### Phase 0: Pre-Upgrade Baseline Testing

**Objective:** Establish baseline before upgrade

**Tests:**
- [ ] Solution builds successfully on net472
- [ ] All existing tests pass
- [ ] Application runs without errors
- [ ] Document baseline performance metrics

**Purpose:** Create rollback reference point

---

#### Phase 1: Post-Upgrade Build Validation

**Objective:** Verify compilation success after atomic upgrade

**Tests:**
- [ ] **Bot.Model** builds without errors
- [ ] **Bot.Services** builds without errors (critical path)
- [ ] **Bot.Console** builds without errors
- [ ] Full solution builds: `dotnet build RickrollBot.sln`
- [ ] No compilation warnings related to deprecated APIs
- [ ] Package restore successful: `dotnet restore RickrollBot.sln`

**Success Criteria:** All projects build with 0 errors

---

#### Phase 2: Comprehensive Testing

##### 2.1. Unit Testing

**Bot.Model Tests:**
- [ ] All model classes serialize/deserialize correctly (Newtonsoft.Json 13.0.4)
- [ ] Graph API models conform to contracts
- [ ] No breaking changes in public APIs

**Bot.Services Tests:**
- [ ] Individual service classes function correctly
- [ ] Dependency injection resolves all services
- [ ] Authentication logic works (JWT validation, MSAL token acquisition)
- [ ] Graph API client initialization successful
- [ ] Bot Framework media handling initialization

**Bot.Console Tests:**
- [ ] Application startup logic works
- [ ] Configuration loading successful
- [ ] Service resolution from DI container

**Test Execution:**
```bash
# If test projects exist:
dotnet test RickrollBot.sln --configuration Release
```

---

##### 2.2. Integration Testing (Critical)

**ASP.NET Core API Endpoints:**
- [ ] All API routes resolve correctly
  - Test each controller endpoint
  - Verify routing attribute mapping
  - Validate request/response serialization
- [ ] CORS middleware works correctly
  - Test cross-origin requests
  - Verify allowed origins/methods/headers
- [ ] Static files serve correctly
  - Test file access
  - Verify content types
- [ ] Error handling works
  - Test 404 responses
  - Test 500 error handling
  - Verify exception logging

**Authentication & Authorization:**
- [ ] JWT token validation works
  - Test with valid token
  - Test with expired token
  - Test with malformed token
- [ ] MSAL token acquisition successful
  - Test client credentials flow
  - Verify token refresh
- [ ] Graph API authentication works
  - Test API calls with acquired token
  - Verify permissions

**Bot Framework Integration:**
- [ ] Skype Bot Media initialization successful
- [ ] Audio/video handling works (NAudio integration)
- [ ] Call handling endpoints respond correctly
- [ ] Media streaming functional
- [ ] Native DLL (Microsoft.Skype.Internal.Media.H264.dll) loads correctly

**Cross-Project Communication:**
- [ ] Bot.Console → Bot.Services integration works
- [ ] Bot.Services → Bot.Model data flow works
- [ ] Dependency injection across projects functional

**Test Scenarios:**
```bash
# Manual API testing examples:
curl -X GET http://localhost:5000/api/calling/status/test-id
curl -X POST http://localhost:5000/api/calling/start -H "Content-Type: application/json" -d '{"callId":"123"}'

# With authentication:
curl -X GET http://localhost:5000/api/calling/status/test-id -H "Authorization: Bearer {token}"
```

---

##### 2.3. Smoke Testing

**Application Startup:**
- [ ] Bot.Console application starts without exceptions
- [ ] Configuration loads from appsettings.json
- [ ] Environment variables read correctly
- [ ] All required services registered in DI container
- [ ] Web host starts successfully
- [ ] Application listens on configured ports

**Core Functionality:**
- [ ] Bot can receive incoming calls/messages
- [ ] Media processing initializes
- [ ] Graph API connectivity established
- [ ] Logging/telemetry functional

**Test Commands:**
```bash
# Start application:
dotnet run --project Bot.Console\Bot.Console.csproj

# Check for startup errors in logs
# Verify no exceptions during initialization
```

---

##### 2.4. Security Validation

**Package Security:**
- [ ] No vulnerable packages: `dotnet list package --vulnerable`
- [ ] No deprecated packages (except intentionally kept): `dotnet list package --deprecated`
- [ ] Newtonsoft.Json updated to 13.0.4 (security fix)

**Authentication Security:**
- [ ] JWT validation uses secure parameters
  - Validate issuer
  - Validate audience
  - Validate signature
  - Check expiration
- [ ] MSAL uses secure token storage
- [ ] No ADAL package remains (deprecated)

**HTTPS/TLS:**
- [ ] Application enforces HTTPS if required
- [ ] Certificate validation works
- [ ] Secure Graph API communication

---

##### 2.5. Performance Testing

**Baseline Comparison:**
- [ ] API response times comparable or improved
  - Measure endpoint latency
  - Compare with Phase 0 baseline
- [ ] Memory usage acceptable
  - Monitor during operation
  - Check for memory leaks
- [ ] CPU usage reasonable
  - Monitor under load
- [ ] Startup time acceptable
  - Measure application cold start
  - Compare with baseline

**Load Testing (if applicable):**
- [ ] Handle expected concurrent requests
- [ ] No performance degradation under load
- [ ] Resource usage scales appropriately

**Performance Metrics to Track:**
- Application startup time (seconds)
- First API call latency (ms)
- Average API response time (ms)
- Memory usage at idle (MB)
- Memory usage under load (MB)

---

##### 2.6. Behavioral Validation

**System.Uri Behavioral Changes:**
- [ ] URL construction produces expected results
- [ ] Relative URI resolution correct
- [ ] Query string parsing works
- [ ] AbsoluteUri values match expectations

**HttpContent Behavioral Changes:**
- [ ] HTTP request/response handling correct
- [ ] Content serialization works
- [ ] Media streaming functional (NAudio)
- [ ] Large file handling works

**Test Approach:**
- Compare outputs with .NET Framework 4.7.2 behavior
- Document any intentional behavioral differences
- Verify no unintended side effects

---

### Validation Sequence (Bottom-Up)

Following dependency order ensures issues caught early:

```
1. Bot.Model (Leaf Node)
   ├─ Build validation
   ├─ Unit tests
   └─ API contract validation
   ✅ PASS → Proceed

2. Bot.Services (Intermediate Node)
   ├─ Build validation
   ├─ Unit tests
   ├─ Integration tests (critical)
   ├─ Authentication tests
   ├─ Bot Framework tests
   └─ Performance tests
   ✅ PASS → Proceed

3. Bot.Console (Root Node)
   ├─ Build validation
   ├─ Application startup
   ├─ End-to-end integration
   ├─ Smoke tests
   └─ Security validation
   ✅ PASS → Complete
```

**If any level fails:** Fix issues at that level before proceeding to next level.

---

### Test Failure Response

**Compilation Failures:**
1. Review breaking changes catalog
2. Apply documented migrations
3. Check for missed API updates
4. Verify package versions correct

**Unit Test Failures:**
1. Identify failing test category
2. Check for behavioral changes
3. Update tests if needed (framework differences)
4. Fix code if actual regression

**Integration Test Failures:**
1. Test ASP.NET Core routing changes
2. Verify middleware pipeline order
3. Check authentication flow
4. Validate serialization settings

**Performance Issues:**
1. Profile with diagnostics tools
2. Check GC settings
3. Verify async/await patterns
4. Review HTTP client usage

---

### Regression Prevention

**Before Declaring Success:**
- [ ] All automated tests pass
- [ ] Manual smoke tests complete
- [ ] No known security issues
- [ ] Performance acceptable
- [ ] Documentation updated
- [ ] No critical warnings in logs

**Test Coverage Verification:**
- [ ] All critical paths tested
- [ ] All API endpoints tested
- [ ] All authentication flows tested
- [ ] All Bot Framework features tested

---

## Source Control Strategy

### Branching Strategy

**Main Branch:** `main`  
**Upgrade Branch:** `upgrade-to-NET10`  
**Source Branch:** `main` (starting point)

#### Branch Structure

```
main (net472 - stable)
 │
 └─> upgrade-to-NET10 (net10.0 - work in progress)
      │
      └─> [Merge back to main when complete]
```

**Workflow:**
1. ✅ Create `upgrade-to-NET10` branch from `main`
2. ✅ All upgrade work happens on `upgrade-to-NET10`
3. ✅ Merge to `main` only after all validations pass
4. ✅ Tag release after successful merge

---

### Commit Strategy (All-At-Once Aligned)

**Recommended: Single Atomic Commit**

The All-At-Once strategy aligns well with a single comprehensive commit:

**Commit Structure:**
```
git add .
git commit -m "Upgrade solution from .NET Framework 4.7.2 to .NET 10.0

- Update all project TargetFrameworks: net472 → net10.0
- Update 17 package references to .NET 10 compatible versions
- Replace 10 incompatible packages (ASP.NET, OWIN, ADAL)
- Remove 2 framework-included packages
- Migrate ASP.NET Framework to ASP.NET Core
  - Convert ApiController → ControllerBase
  - Update routing attributes
  - Replace OWIN middleware with ASP.NET Core equivalents
- Migrate ADAL to MSAL for authentication
- Update JWT token validation API
- Address 141 API compatibility issues
- Fix all compilation errors
- All tests pass

Closes #[issue-number]"
```

**Benefits:**
- Clean Git history (one commit for entire upgrade)
- Easy rollback if needed (single revert)
- Clear boundary between .NET Framework and .NET 10 states
- Aligns with All-At-Once strategy philosophy

---

**Alternative: Logical Checkpoint Commits**

If single commit too large or intermediate checkpoints desired:

**Commit 1: Project Files & Package Updates**
```
git add *.csproj
git commit -m "Update project files and packages for .NET 10.0

- Change TargetFramework to net10.0 (all projects)
- Update package references
- Remove incompatible packages
- Add replacement packages"
```

**Commit 2: Code Changes**
```
git add .
git commit -m "Migrate code to .NET 10 and ASP.NET Core

- Convert ASP.NET Framework to ASP.NET Core
- Migrate ADAL to MSAL
- Update JWT authentication
- Fix compilation errors"
```

**Commit 3: Testing & Validation**
```
git add .
git commit -m "Finalize .NET 10 upgrade and validate

- Fix remaining issues
- Update tests
- Validate functionality
- All tests passing"
```

**Trade-off:** More granular history but harder to rollback partially.

---

### Commit Message Guidelines

**Format:**
```
<type>: <short summary>

<detailed description>

<breaking changes>
<issues closed>
```

**Example for Single Commit:**
```
upgrade: Migrate solution to .NET 10.0 LTS

Complete migration from .NET Framework 4.7.2 to .NET 10.0 using All-At-Once strategy.

Projects Upgraded:
- Bot.Model: net472 → net10.0
- Bot.Services: net472 → net10.0
- Bot.Console: net472 → net10.0

Package Updates:
- Updated 17 packages to .NET 10 versions
- Replaced 10 incompatible packages
- Removed 2 framework-included packages

Major Migrations:
- ASP.NET Framework → ASP.NET Core
- ADAL → MSAL authentication
- OWIN → ASP.NET Core middleware

API Fixes:
- Resolved 82 binary incompatibilities
- Resolved 23 source incompatibilities
- Validated 36 behavioral changes

BREAKING CHANGE: Minimum runtime now .NET 10.0

Closes #[issue-number]
```

---

### Review and Merge Process

#### Pre-Merge Checklist

**Code Quality:**
- [ ] All projects build without errors
- [ ] No compilation warnings
- [ ] Code follows project conventions
- [ ] Breaking changes documented

**Testing:**
- [ ] All unit tests pass
- [ ] All integration tests pass
- [ ] Smoke tests completed
- [ ] Performance validated

**Security:**
- [ ] No vulnerable packages
- [ ] No deprecated authentication
- [ ] Security scan passed

**Documentation:**
- [ ] README updated (if needed)
- [ ] Breaking changes documented
- [ ] Migration notes added

---

#### Pull Request Template

```markdown
## .NET 10.0 Upgrade - All-At-Once Migration

### Summary
Upgrades entire solution from .NET Framework 4.7.2 to .NET 10.0 LTS using All-At-Once strategy.

### Projects Affected
- Bot.Model
- Bot.Services
- Bot.Console

### Key Changes
- ✅ Target framework: net472 → net10.0
- ✅ Package updates: 17 packages
- ✅ ASP.NET Framework → ASP.NET Core
- ✅ ADAL → MSAL authentication
- ✅ 141 API compatibility issues resolved

### Testing Completed
- [x] All projects build successfully
- [x] Unit tests pass
- [x] Integration tests pass
- [x] API endpoints functional
- [x] Authentication works
- [x] Bot Framework integration validated
- [x] Performance acceptable

### Breaking Changes
- Minimum runtime: .NET 10.0 required
- ASP.NET Core hosting model (not IIS-hosted)
- Configuration changes (see migration notes)

### Rollback Plan
Revert this PR to return to .NET Framework 4.7.2.

### Checklist
- [x] Code builds without errors
- [x] Tests pass
- [x] Documentation updated
- [x] Breaking changes noted
- [x] Security validated
```

---

#### Merge Criteria

**Required for Merge:**
- ✅ All builds successful
- ✅ All tests passing
- ✅ Code review approved
- ✅ Security scan clean
- ✅ Performance validated
- ✅ Documentation complete

**Merge Command:**
```bash
git checkout main
git merge --no-ff upgrade-to-NET10
git tag -a v2.0.0-net10 -m "Release: .NET 10.0 upgrade"
git push origin main --tags
```

**Use `--no-ff`:** Preserves upgrade branch history in main.

---

### Post-Merge Actions

**After Successful Merge:**
1. ✅ Delete upgrade branch (optional): `git branch -d upgrade-to-NET10`
2. ✅ Tag release version
3. ✅ Update CI/CD pipelines for .NET 10
4. ✅ Deploy to staging environment
5. ✅ Monitor for issues
6. ✅ Deploy to production (after validation)

**Documentation Updates:**
- Update README with new .NET version requirement
- Update deployment documentation
- Document any configuration changes
- Note behavioral differences (if any)

---

## Success Criteria

### Technical Success Criteria

#### Build & Compilation
- ✅ All 3 projects target `net10.0`
- ✅ Solution builds without errors: `dotnet build RickrollBot.sln`
- ✅ No compilation warnings related to deprecated APIs
- ✅ Package restore successful
- ✅ No package dependency conflicts

#### Package Management
- ✅ All 17 package updates applied
  - Microsoft.Extensions.* packages: 6.0.0 → 10.0.5
  - Newtonsoft.Json: 13.0.1 → 13.0.4
  - Microsoft.Graph.Communications.Calls.Media: 1.2.0.3742 → 1.2.0.15690
  - Microsoft.Skype.Bots.Media: Updated to .NET 10 compatible beta
- ✅ All 10 incompatible packages replaced
  - ASP.NET Framework → ASP.NET Core
  - OWIN → ASP.NET Core middleware
  - ADAL → MSAL
- ✅ 2 framework-included packages removed
- ✅ 8 compatible packages unchanged
- ✅ No security vulnerabilities: `dotnet list package --vulnerable`
- ✅ No deprecated packages (except documented exceptions)

#### API Compatibility
- ✅ 82 binary incompatibilities resolved
- ✅ 23 source incompatibilities resolved
- ✅ 36 behavioral changes validated through testing
- ✅ All API endpoints functional
- ✅ Authentication/authorization works

#### Migration Completeness
- ✅ **Bot.Model** migrated successfully
  - Target framework: net10.0
  - Package updates complete
  - Builds without errors
  - No API issues
- ✅ **Bot.Services** migrated successfully
  - Target framework: net10.0
  - ASP.NET Core migration complete
  - ADAL → MSAL migration complete
  - JWT authentication updated
  - OWIN middleware replaced
  - Builds without errors
  - All 141 API issues resolved
- ✅ **Bot.Console** migrated successfully
  - Target framework: net10.0
  - Package updates complete
  - Application starts without errors
  - Integration functional

---

### Quality Criteria

#### Testing
- ✅ All unit tests pass (if test projects exist)
- ✅ All integration tests pass
- ✅ API endpoint smoke tests pass
- ✅ Authentication flow validated
- ✅ Bot Framework integration functional
- ✅ No regressions identified

#### Code Quality
- ✅ Code follows project conventions
- ✅ No code smells introduced
- ✅ Breaking changes properly handled
- ✅ Error handling maintained
- ✅ Logging functional

#### Performance
- ✅ API response times acceptable (no significant regression)
- ✅ Memory usage reasonable
- ✅ Application startup time acceptable
- ✅ No performance degradation under load

#### Security
- ✅ No security vulnerabilities in packages
- ✅ Authentication secure (JWT, MSAL)
- ✅ No deprecated security libraries (ADAL removed)
- ✅ HTTPS/TLS configuration correct (if applicable)
- ✅ Security best practices followed

---

### Process Criteria

#### Strategy Adherence
- ✅ **All-At-Once Strategy** followed
  - All projects upgraded simultaneously
  - Single atomic upgrade operation
  - No multi-targeting used
  - No intermediate framework states
- ✅ Dependency order respected in validation
  - Bot.Model validated first
  - Bot.Services validated second
  - Bot.Console validated last

#### Source Control
- ✅ All work on `upgrade-to-NET10` branch
- ✅ Commit strategy followed (single commit or logical checkpoints)
- ✅ Commit messages descriptive and complete
- ✅ Breaking changes documented in commits
- ✅ Clean merge to `main` (no conflicts)

#### Documentation
- ✅ Migration notes documented
- ✅ Breaking changes cataloged
- ✅ Package update reference complete
- ✅ README updated (if needed)
- ✅ Configuration changes documented

---

### Validation Checklist

**Final Sign-Off:**

- [ ] ✅ **Build:** Solution builds on .NET 10 without errors
- [ ] ✅ **Packages:** All updates applied, no vulnerabilities
- [ ] ✅ **Tests:** All tests pass (unit, integration, smoke)
- [ ] ✅ **Functionality:** Core features work correctly
- [ ] ✅ **Performance:** No significant regressions
- [ ] ✅ **Security:** No security issues identified
- [ ] ✅ **ASP.NET Core:** Migration complete and functional
- [ ] ✅ **Authentication:** MSAL working, JWT validation correct
- [ ] ✅ **Bot Framework:** Media handling functional
- [ ] ✅ **Source Control:** Clean commit history, ready to merge
- [ ] ✅ **Documentation:** All changes documented

---

### Definition of Done

**The .NET 10.0 upgrade is complete when:**

1. **All technical criteria met** (build, packages, API compatibility)
2. **All quality criteria met** (testing, performance, security)
3. **All process criteria met** (strategy followed, documented)
4. **Validated in dependency order** (Model → Services → Console)
5. **Ready to merge** to `main` branch
6. **Deployment ready** (can be deployed to staging/production)

**At this point:**
- Solution fully operational on .NET 10.0
- All breaking changes resolved
- No known issues or blockers
- Team confident in upgrade quality
- Ready for production use

---

### Post-Upgrade Success Metrics

**Monitor after deployment:**
- Application stability (crash rate, error rate)
- Performance metrics (response times, throughput)
- Resource usage (CPU, memory)
- User-reported issues
- Security incidents (should be zero)

**Success indicators:**
- Stability maintained or improved
- Performance maintained or improved
- No critical bugs introduced
- Team productivity maintained
- Users experience no disruption
