import commandBase = require("commands/commandBase");
import database = require("models/resources/database");
import endpoints = require("endpoints");

class verifyDocumentsIDsCommand extends commandBase {

    constructor(private docIDs: string[], private db: database) {
        super();
    }

    execute(): JQueryPromise<Array<string>> {
        const verifyResult = $.Deferred<Array<string>>();       
        const verifiedIDs: string[] = [];

        if (this.docIDs.length > 0) {
            
            const args = {
                'metadata-only' : true
            }
            const url = endpoints.databases.document.docs + this.urlEncodeArgs(args);

            this.post(url, JSON.stringify(this.docIDs), this.db)
                .fail((xhr: JQueryXHR) => verifyResult.reject(xhr))
                .done((queryResult: queryResultDto<documentDto>) => {
                    if (queryResult && queryResult.Results) {
                        queryResult.Results.forEach(curVerifiedID => {
                            verifiedIDs.push(curVerifiedID['@metadata']['@id']);                        
                        });

                    }
                verifyResult.resolve(verifiedIDs);
            });
            return verifyResult;
        } else {
            verifyResult.resolve(verifiedIDs);
            return verifyResult;
        }
    }
}

export = verifyDocumentsIDsCommand;