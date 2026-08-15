
Backend
```
src/
├── API/             (API-ul tău existent cu Program.cs)
├── Application/     (Logica de business, DTOs, Servicii)
├── Core/            (Entitățile: Song, User, Playlist)
└── Infrastructure/  (EF Core DB Context, PostgreSQL configuration)
```

```
backend/
├── src/
│   ├── Core/                         # Stratul cel mai intern (Fără dependențe externe)
│   │   ├── Domain/                   # Entitățile bazei de date, Enum-uri, Excepții
│   │   │   ├── Entities/             # (ex: Song.cs, Playlist.cs, User.cs)
│   │   │   └── Interfaces/           # Contracte (ex: ISongRepository.cs)
│   │   └── Application/              # Logică de business, Use Cases, DTO-uri
│   │       ├── Dtos/                 # Data Transfer Objects
│   │       ├── Services/             # Logica aplicației (ex: SongService.cs)
│   │       └── Interfaces/           # Interfețe pentru servicii externe
│   │
│   ├── Infrastructure/               # Detalii de implementare tehnică
│   │   ├── Data/                     # EF Core DbContext, Migrații PostgreSQL
│   │   ├── Identity/                 # Implementare Auth, JWT, User Management
│   │   └── ExternalServices/         # Apeluri HTTP către mini-modulele Docker
│   │
│   └── API/                          # Punctul de intrare (Presentation Layer)
│       ├── Controllers/              # Endpoint-uri REST (sau Minimal APIs)
│       ├── Extensions/              # Tratare erori, autentificare, CORS
│       ├── Program.cs                # Dependency Injection (DI) & App Setup
│       └── appsettings.json          # Configurații
│
└── tests/                            # Proiecte separate de Unit / Integration 
```

Apps

```
src/
├── features/
│   ├── player/                       # Tot ce ține de player-ul audio
│   │   ├── components/               # AudioControls.tsx, ProgressBar.tsx
│   │   ├── hooks/                    # useAudioPlayer.ts
│   │   └── playerStore.ts            # Starea (Zustand)
│   ├── search/                       # Tot ce ține de căutare
│   │   ├── components/               # SearchBar.tsx, SearchResults.tsx
│   │   └── useSearchQuery.ts
├── components/                       # Componente UI globale (Button, Modal, 
Input)
├── services/                         # Clientul HTTP (Axios / Fetch către API-ul .NET)
└── types/                            # Interfețe Typescript globale
```