# Wizualizacja definicji i rozwiązania problemu VRP

Aby uruchomić wizualizację wystarczy uruchomić jakiś lekki serwer HTTP, np. ten dostępny jako moduł pythona, w katalogu zawierającym pliki:

```
index.html
index.json
[przykladowe-rozwiazanie].json
```

W pliku `index.json` znajduje się lista dostępnych plików z zagadnieniami VRP:

```json
{ "problems": [{
    "label": "Sample", "file": "sample.json"
}] }
```

Serwer HTTP dostępny w pythonie uruchamimy poleceniem (port 8000 możemy zastąpić innym dostępnym):

```
python -m http.server 8000
```

Następnie pod poniższym adresm odnajdziemy stronę z wizualizacją:
```
http://localhost:8000/
```

Ze względu na to, że wizualizacja wykonuje zapytania AJAXowe do lokalnych plików konieczne jest uruchomienie jej właśnie z poziomu serwera HTTP, a nie bezpośrednio z dysku.

Wizualizacja korzysta z biblioteki leaflet oraz danych OpenStreetMap, a zatem musimy mieć połączenie z Internetem, aby w pełni z niej skorzystać.