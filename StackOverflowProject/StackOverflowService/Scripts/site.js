// Ova funkcija se izvršava kada se dokument učita
$(document).ready(function () {

    // Funkcija za prikaz preview-a slike
    function readURL(input) {
        // Proveravamo da li je korisnik uopšte izabrao fajl
        if (input.files && input.files[0]) {
            var reader = new FileReader();

            // Kada FileReader završi sa čitanjem fajla...
            reader.onload = function (e) {
                // Pronalazimo <img> tag koji treba da ažuriramo i postavljamo mu 'src' na sliku
                var previewImage = $(input).data('preview-for');
                $(previewImage).attr('src', e.target.result);
                $(previewImage).show(); // Prikazujemo <img> tag ako je bio sakriven
            }

            // Čitamo fajl kao Data URL (Base64 string)
            reader.readAsDataURL(input.files[0]);
        }
    }

    // Kačimo 'change' event na sve input[type=file] elemente sa određenom klasom
    // Kada korisnik izabere novi fajl, naša funkcija se poziva
    $(".image-upload-preview").change(function () {
        readURL(this);
    });

});