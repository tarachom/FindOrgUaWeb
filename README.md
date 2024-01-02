# Внутрішній вебсервер
<b>Внутрішній вебсервер для видачі контенту </b> | .net 8, Linux, Windows <br/>

Даний вебсервер віддає контент основному вебсерверу на [Erlang](https://github.com/tarachom/ErlangWeb)<br/>
Працює по протоколу http і привязаний до локальної машини (http://localhost:8082)<br/>

Наприклад

    Новини - http://localhost:8082/news
    Особистості - http://localhost:8082/personality

Для запуску потрібно клонувати репозиторії

    git clone https://github.com/tarachom/FindOrgUaWeb.git
    git clone https://github.com/tarachom/FindOrgUa.git
    git clone https://github.com/tarachom/Configurator3.git
    git clone https://github.com/tarachom/AccountingSoftwareLib.git
    
    cd FindOrgUaWeb
    dotnet build