Odpovede na otázky zo zadania
„Prečo token iba na Task.Run nezastaví už bežiacu slučku?"
Task.Run(..., token) kontroluje token len pri plánovaní – ak je token zrušený pred spustením, task sa vôbec nespustí. Ak už task beží, token v Task.Run sa ignoruje – bežiaca slučka pokračuje, kým sa nedokončí.

Token v Compute (U2a) je preto nutný – kontroluje sa v každom bloku a umožní prerušiť už bežiaci výpočet.

text
Task.Run(() => Compute(bytes, token), token)
           └── kontrola počas behu ──┘  └── kontrola pri plánovaní ──┘
„Prečo samotné async nepresunie hashovanie mimo UI vlákna?"
async/await mení iba spôsob čakania, nie vlákno, na ktorom sa kód vykonáva:

I/O operácie (ReadAsync) – skutočne asynchrónne, vlákno sa uvoľní.

CPU operácie (Fingerprint.Compute) – nie sú asynchrónne, blokujú vlákno. Ak ich nezabalíš do Task.Run, pobelia na UI vlákne a UI zamrzne.

csharp
// ❌ ZLE – Compute blokuje UI vlákno
string hash = Fingerprint.Compute(bytes, token);

// ✅ DOBRE – Compute beží na thread pool vlákne
string hash = await Task.Run(() => Fingerprint.Compute(bytes, token), token);
Task.Run je jediný spôsob, ako presunúť CPU prácu mimo UI vlákna (bez manuálneho Thread).

Overenie
powershell
dotnet test FileDetective.slnx --filter "FullyQualifiedName~U2"
Očakávaný stav: 17/28 (pred U2 bolo 10/28, po U2 pribudne 7 testov).

V aplikácii:

Okno 2 „Nájsť kópie" – 3 skupiny, 4 nadbytočné kópie.

Oba plan.txt – nie sú v žiadnej skupine (rôzny obsah, ako sme si overili).

Premenovaný obrázok – je v skupine kópií.

128 MiB test – pri zrušení počas skenu by sa mal sken prerušiť do niekoľkých stoviek ms (vďaka tokenu v Compute).

Čo si odpovedať (pre seba)
Prečo plan.txt nie sú kópie? Rovnaká veľkosť + rovnaký názov nestačia – treba porovnať obsah (hash).

Prečo LongLength a nie Length? Pre súbory > 2 GB by Length (int) pretečie; LongLength je long a zvládne to.

Prečo token aj v ReadAsync? Aby sa dalo zrušiť aj počas čítania veľkého súboru (128 MiB), nielen počas hashovania.

Prečo ConfigureAwait(false)? Knižnica nemá závisieť od SynchronizationContext volajúceho (inak hrozí deadlock v GUI/WPF).

Čo by sa stalo, keby chýbal Task.Run? Compute by bežal na UI vlákne – UI by zamrzlo na čas výpočtu (pri 128 MiB niekoľko sekúnd).






Odpovede na otázky zo zadania
U3 – „Prečo token iba na Task.Run nezastaví už bežiacu slučku?"
Task.Run(..., token) kontroluje token len pri plánovaní – ak je token zrušený pred spustením, task sa vôbec nespustí. Ak už task beží, token v Task.Run sa ignoruje. Preto je token aj vnútri CPU slučky (Compute) – aby sa dala prerušiť už bežiaca práca.

U3 – „Prečo samotné async nepresunie hashovanie mimo UI vlákna?"
async/await mení iba spôsob čakania, nie vlákno. I/O operácie (ReadAsync) sú skutočne asynchrónne, ale CPU práca (Compute) nie je – blokuje vlákno. Bez Task.Run by bežala na UI vlákne a UI by zamrzlo. Preto sa CPU práca balí do Task.Run.

U4 – „Čo sa pokazí pri predčasnom uvoľnení registrácie?"
Ak sa registrácia uvoľní pred await, token sa odregistruje – zrušenie tokenu už nezavolá TrySetCanceled. WaitAsync by čakal navždy (alebo kým nepríde TryDecide). Preto using var registration musí byť na úrovni metódy, aby žila cez await.

Zhrnutie
Metóda	                    Kľúčové	                                                          Prečo
RunAsync	using var linked = CreateLinkedTokenSource(...)	                    živý počas await, spája oba zdroje
RunAsync	catch (OCE) { if (userToken...) ... if (deadlineToken...) ... }	    rozlíšenie príčiny, používateľ má prednosť
RunAsync	throw new OperationCanceledException(userToken)	                    OCE nesie pôvodný token
RunAsync	throw new TimeoutException(...)	                                    deadline → TimeoutException
WaitAsync	using var registration = token.Register(...)                    	živá počas await
WaitAsync	TrySetCanceled(token)	                                            zrušenie → TaskCanceledException
TryDecide	TrySetResult(save)	                                                prvé vyhrá, druhé vráti false



