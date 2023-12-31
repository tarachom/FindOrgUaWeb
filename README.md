# Внутрішній вебсервер
<b>Внутрішній вебсервер для видачі контенту </b> | .net 8, Linux, Windows <br/>

Даний вебсервер віддає контент основному вебсерверу на [Erlang](https://github.com/tarachom/ErlangWeb)<br/>
Працює по протоколу http і привязаний до локальної машини (http://localhost:8082)<br/>

Наприклад

    Новини - http://localhost:8082/news
    Особистості - http://localhost:8082/personality

Скрипт запустку двох вебсерверів (внутрішнього та Erlang)

    #DotNet
    coproc dotnet run --project /home/tarachom/Projects/FindOrgUaWeb/FindOrgUaWeb.csproj
    
    #Erlang
    cd /home/tarachom/www/
    erl -s httpnet
