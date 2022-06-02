const tokenKey = "accessToken";

document.getElementById("submitLogin").addEventListener("click", async e => {
    e.preventDefault();
    const form = document.forms["loginForm"];
    const response = await fetch("/login", {
        method: "POST",
        headers: { "Accept": "application/json", "Content-Type": "application/json" },
        body: JSON.stringify({
            email: form.elements["email"].value,
            password: form.elements["password"].value
        })
    });

    if (response.ok === true) {
        const data = await response.json();
        document.getElementById("userName").innerText = data.username;
        document.getElementById("userInfo").style.display = "block";
        form.style.display = "none";
        sessionStorage.setItem(tokenKey, data.access_token);
    } else {
        alert("Status: " + response.status);
    }
});

document.getElementById("getData").addEventListener("click", async e => {
    e.preventDefault();
    const token = sessionStorage.getItem(tokenKey);
    const response = await fetch("/data", {
        method: "GET",
        headers: {
            "Accept": "application/json",
            "Authorization": "Bearer " + token
        }
    });

    if (response.ok === true) {
        const data = await response.json();
        alert(data.message);
    } else {
        alert("Status: " + response.status);
    }
});

document.getElementById("logOut").addEventListener("click", e => {
    e.preventDefault();
    const form = document.forms["loginForm"];
    document.getElementById("userName").innerText = "";
    document.getElementById("userInfo").style.display = "none";
    form.style.display = "block";
    sessionStorage.removeItem(tokenKey);
});