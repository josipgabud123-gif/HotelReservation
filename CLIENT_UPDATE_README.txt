CLIENT UPDATE v4

- Public search prikazuje sve fotografije smjestaja i sve fotografije dostupnih soba.
- Owner moze dodavati, brisati i postavljati primarnu fotografiju hotelske sobe.
- Owner prikaz jasno pokazuje sobe, slike, amenities, availability blockove i rezervacije.
- Admin moze otvoriti detalje smjestaja i vidjeti slike/sobe prije odluke.
- Nakon create/update/delete/activate/deactivate akcija relevantni prikaz se automatski osvjezava.
- Slike se mogu otvoriti u punoj velicini.
- Ako fizicka datoteka slike ne postoji u wwwroot/uploads, UI prikazuje jasnu poruku umjesto broken-image ikone.

VAZNO ZA POSTOJECE SLIKE:
Baza sprema putanju slike, a sama slika je fizicka datoteka u HotelReservation.API/wwwroot/uploads.
Ako premjestis projekt u novi folder ili zamijenis cijeli projekt starijim ZIP-om, kopiraj svoj postojeci wwwroot/uploads folder.
Changed-files ZIP namjerno NE sadrzi uploads folder kako ne bi pregazio tvoje postojece slike.
